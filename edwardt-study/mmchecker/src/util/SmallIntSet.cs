using System;
using System.Collections.Generic;
using System.Text;

namespace mmchecker.util
{
    /// <summary>
    /// A fast and simple int set implementation for set with very small number of elements
    /// </summary>
    class SmallIntSet 
    {
        int[] elements;
        int size;

        public SmallIntSet()
        {
            elements = new int[10];
            size = 0;
        }

        public int Size
        {
            get { return size; } 
        }

        public void Add(int item)
        {
            for (int i = 0; i < size; i++)
                if (elements[i] == item)
                    return;
            if (elements.Length == size)
            {
                int[] newArray = new int[size + 10];
                for (int i = 0; i < size; i++)
                    newArray[i] = elements[i];
                elements = newArray;
            }
            elements[size] = item;
            size++;
        }

        public bool Has(int item)
        {
            for (int i = 0; i < size; i++)
                if (elements[i] == item)
                    return true;
            return false;
        }

        public void Clear()
        {
            size = 0;
        }

        public int[] GetElements()
        {
            int[] ret = new int[size];
            for (int i = 0; i < size; i++)
                ret[i] = elements[i];
            return ret;
        }
    }
}
