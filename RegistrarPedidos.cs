using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualBasic;

public class MyQueue<Type>
{
    private Type[] _values;
    private Type[] Values
    {
        get
        {
            return _values;
        }
        set
        {
            if (value is null)
            {
                throw new Exception("La queue siempre tiene que tener una instancia de objetos o algo asi creo");
            }
            _values = value;
        }

    }

    private readonly int _capacity;
    public int Capacity
    {
        get
        {
            return _capacity;
        }
        init
        {
            if (value <= 0)
            {
                throw new Exception("La capacidad no puede ser menor o igual a cero");
            }
            _capacity = value;
        }
    }

    private int _count;
    public int Count
    {
        get
        {
            return _count;
        }
        private set
        {
            if (value > Capacity)
            {
                throw new Exception("La cuenta de elementos no puede ser mayor a la capacidad");
            }
            else if (value < 0)
            {
                throw new Exception("La cuenta de elementos no puede ser menor a cero");
            }
            _count = value;
        }
    }

    public bool IsFull
    {
        get
        {
            return Count == Capacity;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return Count == 0;
        }
    }

    private static int _defaultCapacity = 5;
    private static int DefaultCapacity
    {
        get
        {
            return _defaultCapacity;
        }
        set
        {
            if (value <= 0)
            {
                throw new Exception("La capacidad no puede ser menor o igual a cero");
            }
            _defaultCapacity = value;
        }
    }

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public MyQueue() : this(DefaultCapacity)
    {
        //aqui funciona como el constructor de abajo pero se le pasa la capacidad default si es que no se la dan supongo..
    }

    public MyQueue(int maxCapacity)
    {
        Capacity = maxCapacity;
        Values = new Type[maxCapacity];
        Count = 0;
    }

    public MyQueue(Type[] valoresIniciales)
    : this(valoresIniciales.Length)
    {
        // Capacidad = valoresIniciales.Length;
        // Valores = new int[Capacidad];
        // Cuenta = 0;

        foreach (var value in valoresIniciales)
        {
            Push(value);
        }
    }

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public void Push(Type value)
    {
        try
        {
            Values[Count] = value;
            Count++;
        }
        catch (Exception nombreQueMeDeLaGanaDeMiExcepcion)
        {
            Console.WriteLine(nombreQueMeDeLaGanaDeMiExcepcion);
        }
    }

    public Type Pop()
    {
        if (this.IsEmpty)
        {
            throw new Exception("La queue esta vacia no se puede poppear");
        }

        Type temp = Values[0];
        for (int i = 1; i < Count; i++)
        {
            Values[i - 1] = Values[i];
        }

        Count--;
        return temp;
    }

    public Type Peek()
    {
        if (this.IsEmpty)
        {
            throw new Exception("La queue esta vacia no se puede poppear");
        }
        return Values[0];
    }

    public void ExaminarQueue()
    {
        Console.WriteLine($"Capacity: {Capacity}");
        Console.WriteLine($"Count: {Count}");
    }
}