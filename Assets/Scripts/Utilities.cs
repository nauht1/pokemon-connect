using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static void Shuffle<T>(this List<T> list) // sử dụng this để biến Shuffle thành extension method của List<T>
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
