﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentNHibernate.Utils;

namespace FluentNHibernate
{
    public abstract class Member : IEquatable<Member>
    {
        public abstract string Name { get; }
        public abstract Type PropertyType { get; }
        public abstract bool CanWrite { get; }
        public abstract MemberInfo MemberInfo { get; }
        public abstract Type DeclaringType { get; }
        public abstract bool HasIndexParameters { get; }
        public abstract bool IsMethod { get; }
        public abstract bool IsField { get; }
        public abstract bool IsProperty { get; }
        public abstract bool IsPrivate { get; }
        public abstract bool IsProtected { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsInternal { get; }

        public bool Equals(Member other)
        {
            return other.MemberInfo.MetadataToken.Equals(MemberInfo.MetadataToken);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is Member)) return false;
            return Equals((Member)obj);
        }

        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode() ^ 3;
        }

        public static bool operator ==(Member left, Member right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Member left, Member right)
        {
            return !Equals(left, right);
        }

        public abstract void SetValue(object target, object value);
        public abstract object GetValue(object target);
    }

    internal class FieldMember : Member
    {
        private readonly FieldInfo member;

        public override void SetValue(object target, object value)
        {
            member.SetValue(target, value);
        }

        public override object GetValue(object target)
        {
            return member.GetValue(target);
        }

        public FieldMember(FieldInfo member)
        {
            this.member = member;
        }

        public override string Name
        {
            get { return member.Name; }
        }
        public override Type PropertyType
        {
            get { return member.FieldType; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }
        public override MemberInfo MemberInfo
        {
            get { return member; }
        }
        public override Type DeclaringType
        {
            get { return member.DeclaringType; }
        }
        public override bool HasIndexParameters
        {
            get { return false; }
        }
        public override bool IsMethod
        {
            get { return false; }
        }
        public override bool IsField
        {
            get { return true; }
        }
        public override bool IsProperty
        {
            get { return false; }
        }

        public override bool IsPrivate
        {
            get { return member.IsPrivate; }
        }

        public override bool IsProtected
        {
            get { return member.IsFamily || member.IsFamilyAndAssembly; }
        }

        public override bool IsPublic
        {
            get { return member.IsPublic; }
        }

        public override bool IsInternal
        {
            get { return member.IsAssembly || member.IsFamilyAndAssembly; }
        }
    }

    internal class MethodMember : Member
    {
        private readonly MethodInfo member;

        public override void SetValue(object target, object value)
        {
            throw new NotSupportedException("Cannot set the value of a method Member.");
        }

        public override object GetValue(object target)
        {
            return member.Invoke(target, null);
        }

        public MethodMember(MethodInfo member)
        {
            this.member = member;
        }

        public override string Name
        {
            get { return member.Name; }
        }
        public override Type PropertyType
        {
            get { return member.ReturnType; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override MemberInfo MemberInfo
        {
            get { return member; }
        }
        public override Type DeclaringType
        {
            get { return member.DeclaringType; }
        }
        public override bool HasIndexParameters
        {
            get { return false; }
        }
        public override bool IsMethod
        {
            get { return true; }
        }
        public override bool IsField
        {
            get { return false; }
        }
        public override bool IsProperty
        {
            get { return false; }
        }

        public override bool IsPrivate
        {
            get { return member.IsPrivate; }
        }

        public override bool IsProtected
        {
            get { return member.IsFamily || member.IsFamilyAndAssembly; }
        }

        public override bool IsPublic
        {
            get { return member.IsPublic; }
        }

        public override bool IsInternal
        {
            get { return member.IsAssembly || member.IsFamilyAndAssembly; }
        }
    }

    public static class AndAndExtention
    {
        public static Return AndAnd<This, Return>(this This instance, Func<This, Return> evaluate)
            where This : class
        {
            if (instance != null)
                return evaluate(instance);

            return default(Return);
        }
    }

    internal class PropertyMember : Member
    {
        private readonly PropertyInfo member;
        MethodMember getMethod;
        MethodMember setMethod;

        public PropertyMember(PropertyInfo member)
        {
            this.member = member;
            getMethod = member.GetGetMethod().AndAnd(_ => (MethodMember)_.ToMember());
            setMethod = member.GetSetMethod().AndAnd(_ => (MethodMember)_.ToMember());
        }

        public override void SetValue(object target, object value)
        {
            member.SetValue(target, value, null);
        }

        public override object GetValue(object target)
        {
            return member.GetValue(target, null);
        }

        public override string Name
        {
            get { return member.Name; }
        }
        public override Type PropertyType
        {
            get { return member.PropertyType; }
        }
        public override bool CanWrite
        {
            get { return member.CanWrite; }
        }
        public override MemberInfo MemberInfo
        {
            get { return member; }
        }
        public override Type DeclaringType
        {
            get { return member.DeclaringType; }
        }
        public override bool HasIndexParameters
        {
            get { return member.GetIndexParameters().Length > 0; }
        }
        public override bool IsMethod
        {
            get { return false; }
        }
        public override bool IsField
        {
            get { return false; }
        }
        public override bool IsProperty
        {
            get { return true; }
        }

        public override bool IsPrivate
        {
            get { return getMethod.IsPrivate; }
        }

        public override bool IsProtected
        {
            get { return getMethod.IsProtected; }
        }

        public override bool IsPublic
        {
            get { return getMethod.IsPublic; }
        }

        public override bool IsInternal
        {
            get { return getMethod.IsInternal; }
        }

        public MethodMember Get
        {
            get { return getMethod; }
        }

        public MethodMember Set
        {
            get { return setMethod; }
        }
    }

    public static class MemberExtensions
    {
        public static Member ToMember(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new NullReferenceException("Cannot create member from null.");
            
            return new PropertyMember(propertyInfo);
        }

        public static Member ToMember(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new NullReferenceException("Cannot create member from null.");

            return new MethodMember(methodInfo);
        }

        public static Member ToMember(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new NullReferenceException("Cannot create member from null.");

            return new FieldMember(fieldInfo);
        }

        public static Member ToMember(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new NullReferenceException("Cannot create member from null.");

            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).ToMember();
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).ToMember();
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ToMember();

            throw new InvalidOperationException("Cannot convert MemberInfo '" + memberInfo.Name + "' to Member.");
        }

        public static IEnumerable<Member> GetInstanceFields(this Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (!field.Name.StartsWith("<"))
                    yield return field.ToMember();
        }

        public static IEnumerable<Member> GetInstanceMethods(this Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
                    yield return method.ToMember();
        }

        public static IEnumerable<Member> GetInstanceProperties(this Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                yield return property.ToMember();
        }

        public static IEnumerable<Member> GetInstanceMembers(this Type type)
        {
            var members = new HashSet<Member>(new MemberEqualityComparer());

            type.GetInstanceProperties().Each(x => members.Add(x));
            type.GetInstanceFields().Each(x => members.Add(x));
            type.GetInstanceMethods().Each(x => members.Add(x));

            if (type.BaseType != typeof(object))
                type.BaseType.GetInstanceMembers().Each(x => members.Add(x));

            return members;
        }
    }

    public class MemberEqualityComparer : IEqualityComparer<Member>
    {
        public bool Equals(Member x, Member y)
        {
            return x.MemberInfo.MetadataToken.Equals(y.MemberInfo.MetadataToken);
        }

        public int GetHashCode(Member obj)
        {
            return obj.MemberInfo.MetadataToken.GetHashCode();
        }
    }
}
