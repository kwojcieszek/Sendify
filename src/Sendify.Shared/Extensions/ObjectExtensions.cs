namespace Sendify.Shared.Extensions;

public static class ObjectExtensions
{
    public static T GetPropertyValueByName<T>(this object obj,string name)
    {
        var t= obj?.GetType()!.GetProperty(name)!.GetValue(obj);

        return t!=null ? (T)t : default(T);
    }

    public static void SetPropertyValueByName<T>(this object obj, string name,object value)
    {
        obj?.GetType()!.GetProperty(name)!.SetValue(obj, value);
    }
}