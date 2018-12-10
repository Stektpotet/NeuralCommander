using System.Collections.Generic;
using System.Linq;


public static class CollectionExtensions
{
    //Having an 'Add'-function on IEnumerables allows them to have collection initializers/initializer lists
    #region Add
    public static void Add<T>( this Stack<T> stack, T item) => stack.Push(item); 
    public static void Add<T>( this Queue<T> queue, T item ) => queue.Enqueue(item);
    public static void Add<T>( this LinkedList<T> lList, T item ) => lList.AddLast(item);
    #endregion

    public static T GetRandom<T>( this IEnumerable<T> collection, System.Random picker ) => collection.ElementAt(picker.Next(0, collection.Count()));
    public static T GetRandom<T>( this IList<T> collection, System.Random picker ) => collection.ElementAt(picker.Next(0, collection.Count));
    public static T GetRandom<T>( this T[] collection, System.Random picker ) => collection.ElementAt(picker.Next(0, collection.Length));

    public static void Clear<T>(this T[] arr) => System.Array.Clear(arr, 0, arr.Length);

}
