﻿using System;
using System.ComponentModel;
using System.Reflection;

public static class EnumExtensions {
    public static string GetDescription(this Enum enumValue) {
        FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
        DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attribute == null ? enumValue.ToString() : attribute.Description;
    }
}
