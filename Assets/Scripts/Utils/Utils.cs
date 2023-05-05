/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Multi-dimensional Array Util Classes

// Generic 2D array class
public class Array2D<T>
{
    public readonly T[] Array;
    public readonly int Width;
    public readonly int Height;

    public Array2D(int width, int height)
    {
        Width = width;
        Height = height;

        Array = new T[height * width];
    }

    /// <summary>
    /// Access array by 2D coordinates.
    /// </summary>
    /// <param name="r">Row</param>
    /// <param name="c">Column</param>
    /// <returns></returns>
    public T this[int r, int c]
    {
        get => Array[r * Width + c];
        set => Array[r * Width + c] = value;
    }
}

// Generic 3D array class
public class Array3D<T>
{
    public readonly T[] Array;
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;

    public Array3D(int width, int height, int depth)
    {
        Width = width;
        Height = height;
        Depth = depth;

        Array = new T[height * width * depth];
    }

    /// <summary>
    /// Access array by 3D coordinates.
    /// </summary>
    /// <param name="r">Row</param>
    /// <param name="c">Column</param>
    /// <param name="d">Depth</param>
    /// <returns></returns>
    public T this[int r, int c, int d]
    {
        get => Array[r * (Width * Depth) + c * Depth + d];
        set => Array[r * (Width * Depth) + c * Depth + d] = value;
    }
}
#endregion

public class Utils : CSingleton<Utils>
{   
    /// <summary>
    /// Applies a 3D Gaussian filter to an input image
    /// </summary>
    /// <param name="img">img size is(w,h) here</param>
    /// <param name="k_size">Size of the kernel</param>
    /// <param name="sigma">Sigma parameter used for Gaussian filtering</param>
    public Array3D<float> Gaussian_Filter3d(Array3D<float> img, int k_size = 3, float sigma = 1f)
    {
        int w = img.Width;
        int h = img.Height;
        int c = img.Depth;
        Array3D<float> output = new Array3D<float>(w, h, c);

        // Creates kernel with zero padding for calculations
        Array2D<float> kernel = new Array2D<float>(k_size, k_size);
        int pad = k_size / 2;
        float sigsq_inv = 1 / (2 * sigma * sigma);
        float divide_value = sigsq_inv * (1 / Mathf.PI);
        float kernel_sum = 0;
        for (int x = -pad; x < k_size - pad; x++)
        {
            for (int y = -pad; y < k_size - pad; y++)
            {
                float newVal = Mathf.Exp(-(x * x + y * y) * sigsq_inv) * divide_value;
                kernel[x + pad, y + pad] = newVal;
                kernel_sum += newVal;
            }
        }
        
        // Dividing kernel by kernel sum to normalize kernel sum to 1
        for (int x = 0; x < k_size; x++)
        {
            for (int y = 0; y < k_size; y++)
            {
                kernel[x, y] /= kernel_sum;
            }
        }

        // Apply Gaussian filter
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int z = 0; z < c; z++)
                {
                    if ((x < pad) || (y < pad) || (x > (w - pad - 1)) || (y > (h - pad - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        // Calculate kernel sum 
                        float sum = 0;
                        for (int kx = -pad; kx < k_size - pad; kx++)
                        {
                            for (int ky = -pad; ky < k_size - pad; ky++)
                            {
                                sum += img[kx + x, ky + y, z] * kernel[kx + pad, ky + pad];
                            }
                        }
                        output[x, y, z] = sum;
                    }
                }  
            }
        }

        return output;
    }

    /// <summary>
    /// Multiply a list by a float
    /// </summary>
    /// <param name="listA">Input list</param>
    /// <param name="b">Input float</param>
    public List<float> ListMultiply(List<float> listA, float b)
    {
        List<float> new_list = new List<float>();
        for (int i = 0; i < listA.Count; i++)
        {
            float a = listA[i] * b;
            new_list.Add(a);
        }
        return new_list;
    }


    /// <summary>
    /// Adds two lists of the same length
    /// </summary>
    /// <param name="listA">Input list A</param>
    /// <param name="listB">Input list B</param>
    public List<float> ListAdd(List<float> listA, List<float> listB)
    {
        List<float> new_list = new List<float>();
        for (int i = 0; i < listA.Count; i++)
        {
            float a = listA[i] + listB[i];
            new_list.Add(a);
        }
        return new_list;
    }

    /// <summary>
    /// get list sum
    /// </summary>
    /// <param name="listA"></param>
    /// <returns></returns>
    public float ListSum(List<float> listA)
    {
        float sum = 0;
        for(int i = 0; i < listA.Count; i++)
        {
            sum += listA[i];
        }
        return sum;
    }

    /// <summary>
    /// count how many value in A larger than value
    /// </summary>
    /// <param name="listA"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int CountLarger(List<float> listA, float value)
    {
        int count = 0;
        for(int i = 0; i < listA.Count; i++)
        {
            if(listA[i] > value)
            {
                count++;
            }
        }
        return count;
    }

    #region Print Functions
    /// <summary>
    /// print list
    /// </summary>
    /// <param name="l">List to print</param>
    public void PrintList(IEnumerable l)
    {
        string s = "";
        foreach(var i in l)
        {
            s += i + " ";
        }
        Debug.Log(s);
    }

    /// <summary>
    /// Prints heatmap[:,:,0]
    /// </summary>
    /// <param name="heatmap">Heatmap to print</param>
    public void PrintHeatmap(float[,,] heatmap)
    {
        string p = "";
        for (int y = 0; y < heatmap.GetLength(1); y++)
        {
            for (int x = 0; x < heatmap.GetLength(0); x++)
            {
                p += heatmap[x, y, 0].ToString() + " ";
            }
            p += "\n";
        }
    }
    #endregion
}
