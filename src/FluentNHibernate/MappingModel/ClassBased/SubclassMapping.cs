﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Utils;
using FluentNHibernate.Visitors;

namespace FluentNHibernate.MappingModel.ClassBased
{
    public class SubclassMapping : ClassMappingBase, ITopMapping
    {
        public SubclassType SubclassType { get; private set; }
        private AttributeStore<SubclassMapping> attributes;

        public SubclassMapping(SubclassType subclassType)
            : this(subclassType, new AttributeStore())
        {}

        public SubclassMapping(SubclassType subclassType, AttributeStore underlyingStore)
        {
            SubclassType = subclassType;
            attributes = new AttributeStore<SubclassMapping>(underlyingStore);
        }

        /// <summary>
        /// Set which type this subclass extends.
        /// Note: This doesn't actually get output into the XML, it's
        /// instead used as a marker for the <see cref="SeparateSubclassVisitor"/>
        /// to pair things up.
        /// </summary>
        public Type Extends
        {
            get { return attributes.Get(x => x.Extends); }
            set { attributes.Set(x => x.Extends, value); }
        }

        public void ChangeSubclassType(SubclassType type)
        {
            SubclassType = type;
            Subclasses.Each(x => x.ChangeSubclassType(type));
        }

        public override void AcceptVisitor(IMappingModelVisitor visitor)
        {
            visitor.ProcessSubclass(this);

            if (SubclassType == SubclassType.JoinedSubclass && Key != null)
                visitor.Visit(Key);

            base.AcceptVisitor(visitor);
        }

        public override void MergeAttributes(AttributeStore store)
        {
            attributes.Merge(new AttributeStore<SubclassMapping>(store));
        }

        public override string Name
        {
            get { return attributes.Get(x => x.Name); }
            set { attributes.Set(x => x.Name, value); }
        }

        public override Type Type
        {
            get { return attributes.Get(x => x.Type); }
            set { attributes.Set(x => x.Type, value); }
        }

        public object DiscriminatorValue
        {
            get { return attributes.Get(x => x.DiscriminatorValue); }
            set { attributes.Set(x => x.DiscriminatorValue, value); }
        }

        public bool Lazy
        {
            get { return attributes.Get(x => x.Lazy); }
            set { attributes.Set(x => x.Lazy, value); }
        }

        public string Proxy
        {
            get { return attributes.Get(x => x.Proxy); }
            set { attributes.Set(x => x.Proxy, value); }
        }

        public bool DynamicUpdate
        {
            get { return attributes.Get(x => x.DynamicUpdate); }
            set { attributes.Set(x => x.DynamicUpdate, value); }
        }

        public bool DynamicInsert
        {
            get { return attributes.Get(x => x.DynamicInsert); }
            set { attributes.Set(x => x.DynamicInsert, value); }
        }

        public bool SelectBeforeUpdate
        {
            get { return attributes.Get(x => x.SelectBeforeUpdate); }
            set { attributes.Set(x => x.SelectBeforeUpdate, value); }
        }

        public bool Abstract
        {
            get { return attributes.Get(x => x.Abstract); }
            set { attributes.Set(x => x.Abstract, value); }
        }

        public string EntityName
        {
            get { return attributes.Get(x => x.EntityName); }
            set { attributes.Set(x => x.EntityName, value); }
        }

        public string TableName
        {
            get { return attributes.Get(x => x.TableName); }
            set { attributes.Set(x => x.TableName, value); }
        }

        public KeyMapping Key
        {
            get { return attributes.Get(x => x.Key); }
            set { attributes.Set(x => x.Key, value); }
        }

        public string Check
        {
            get { return attributes.Get(x => x.Check); }
            set { attributes.Set(x => x.Check, value); }
        }

        public string Schema
        {
            get { return attributes.Get(x => x.Schema); }
            set { attributes.Set(x => x.Schema, value); }
        }

        public string Subselect
        {
            get { return attributes.Get(x => x.Subselect); }
            set { attributes.Set(x => x.Subselect, value); }
        }

        public TypeReference Persister
        {
            get { return attributes.Get(x => x.Persister); }
            set { attributes.Set(x => x.Persister, value); }
        }

        public int BatchSize
        {
            get { return attributes.Get(x => x.BatchSize); }
            set { attributes.Set(x => x.BatchSize, value); }
        }

        public override bool IsSpecified(string property)
        {
            return attributes.IsSpecified(property);
        }

        public bool HasValue<TResult>(Expression<Func<SubclassMapping, TResult>> property)
        {
            return attributes.HasValue(property);
        }

        public void SetDefaultValue<TResult>(Expression<Func<SubclassMapping, TResult>> property, TResult value)
        {
            attributes.SetDefault(property, value);
        }

        public void OverrideAttributes(AttributeStore store)
        {
            attributes = new AttributeStore<SubclassMapping>(store);
        }

        public bool Equals(SubclassMapping other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other.attributes, attributes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as SubclassMapping);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                {
                    return (base.GetHashCode() * 397) ^ (attributes != null ? attributes.GetHashCode() : 0);
                }
            }
        }

        public void AddTo(HibernateMapping hbm)
        {
            hbm.AddSubclass(this);
        }

        public IEnumerable<Member> GetUsedMembers()
        {
            var memberMappings = new List<IMemberMapping>();
            
            References.Each(memberMappings.Add);
            Collections.Each(memberMappings.Add);
            Properties.Each(memberMappings.Add);
            Components.Each(memberMappings.Add);
            OneToOnes.Each(memberMappings.Add);
            Anys.Each(memberMappings.Add);

            return memberMappings
                .Select(x => x.Member);
        }

        public void AddMappedMember(IMemberMapping mapping)
        {
            if (mapping is ManyToOneMapping)
                AddReference((ManyToOneMapping)mapping);
            if (mapping is ICollectionMapping)
                AddCollection((ICollectionMapping)mapping);
            if (mapping is PropertyMapping)
                AddProperty((PropertyMapping)mapping);
            if (mapping is IComponentMapping)
                AddComponent((IComponentMapping)mapping);
            if (mapping is OneToOneMapping)
                AddOneToOne((OneToOneMapping)mapping);
            if (mapping is AnyMapping)
                AddAny((AnyMapping)mapping);
        }
    }
}