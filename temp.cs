using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// =======================
//   MODELOS DE DOMINIO
// =======================

public class Pedido
{
    public int ProductId { get; set; }
    public int Cantidad { get; set; }
    public DateTime CreatedAt { get; set; }

    public Pedido()
    {
        ProductId = 0;
        Cantidad = 0;
        CreatedAt = DateTime.Now;
    }

    public Pedido(int productId, int cantidad)
    {
        ProductId = productId;
        Cantidad = cantidad;
        CreatedAt = DateTime.Now;
    }

    public override string ToString()
    {
        return "Pedido -> Prod:" + ProductId + " Qty:" + Cantidad + " (" + CreatedAt.ToString("HH:mm:ss") + ")";
    }
}

public class Lote
{
    public int ProductId { get; set; }
    public int Unidades { get; set; }
    public DateTime ReceivedAt { get; set; }

    public Lote()
    {
        ProductId = 0;
        Unidades = 0;
        ReceivedAt = DateTime.Now;
    }

    public Lote(int productId, int unidades)
    {
        ProductId = productId;
        Unidades = unidades;
        ReceivedAt = DateTime.Now;
    }

    public override string ToString()
    {
        return "Lote -> Prod:" + ProductId + " Units:" + Unidades + " (" + ReceivedAt.ToString("HH:mm:ss") + ")";
    }
}

// ===================================
//   TUS ESTRUCTURAS (EXTENDIDAS)
// ===================================
public class AnotherQueue<T>
{
    private T[] items;
    private int count;
    private int capacity;

    public AnotherQueue()
    {
        capacity = 32;
        items = new T[capacity];
        count = 0;
    }

    public AnotherQueue(int capacidad)
    {
        capacity = capacidad;
        items = new T[capacidad];
        count = 0;
    }

    public int Count
    {
        get
        {
            return count;
        }
    }

    public int Capacity
    {
        get
        {
            return capacity;
        }
    }

    public void Enqueue(T item)
    {
        if (count >= capacity)
        {
            throw new Exception("La cola está llena");
        }
        items[count] = item;
        count = count + 1;
    }

    public T Dequeue()
    {
        if (count == 0)
        {
            throw new Exception("La cola está vacía");
        }

        T primero = items[0];
        for (int i = 1; i < count; i++)
        {
            items[i - 1] = items[i];
        }
        items[count - 1] = default(T);
        count = count - 1;
        return primero;
    }

    public T Peek()
    {
        if (count == 0)
        {
            throw new Exception("La cola está vacía");
        }
        return items[0];
    }

    // Para guardar al JSON (exporta en orden FIFO)
    public List<T> ToList()
    {
        List<T> copia = new List<T>();
        for (int i = 0; i < count; i++)
        {
            copia.Add(items[i]);
        }
        return copia;
    }

    // Para reconstruir desde JSON (en el mismo orden)
    public static AnotherQueue<T> FromList(List<T> datos, int capacidadExtra)
    {
        int cap = (datos == null ? 0 : datos.Count) + capacidadExtra;
        if (cap < 1) cap = 1;

        AnotherQueue<T> q = new AnotherQueue<T>(cap);
        if (datos != null)
        {
            for (int i = 0; i < datos.Count; i++)
            {
                q.Enqueue(datos[i]);
            }
        }
        return q;
    }
}

public class AnotherStack<T>
{
    private T[] items;
    private int count;
    private int capacity;

    public AnotherStack()
    {
        capacity = 32;
        items = new T[capacity];
        count = 0;
    }

    public AnotherStack(int capacidad)
    {
        capacity = capacidad;
        items = new T[capacidad];
        count = 0;
    }

    public int Count
    {
        get
        {
            return count;
        }
    }

    public int Capacity
    {
        get
        {
            return capacity;
        }
    }

    public void Push(T item)
    {
        if (count >= capacity)
        {
            throw new Exception("La pila está llena");
        }
        items[count] = item;
        count = count + 1;
    }

    public T Pop()
    {
        if (count == 0)
        {
            throw new Exception("La pila está vacía");
        }
        T val = items[count - 1];
        items[count - 1] = default(T);
        count = count - 1;
        return val;
    }

    public T Peek()
    {
        if (count == 0)
        {
            throw new Exception("La pila está vacía");
        }
        return items[count - 1];
    }

    // Para guardar al JSON (exporta de abajo hacia arriba: índice 0 = fondo, último = cima)
    public List<T> ToListBottomToTop()
    {
        List<T> copia = new List<T>();
        for (int i = 0; i < count; i++)
        {
            copia.Add(items[i]);
        }
        return copia;
    }

    // Reconstruye desde lista fondo->cima (haciendo Push en ese orden)
    public static AnotherStack<T> FromListBottomToTop(List<T> datos, int capacidadExtra)
    {
        int cap = (datos == null ? 0 : datos.Count) + capacidadExtra;
        if (cap < 1) cap = 1;

        AnotherStack<T> s = new AnotherStack<T>(cap);
        if (datos != null)
        {
            for (int i = 0; i < datos.Count; i++)
            {
                s.Push(datos[i]);
            }
        }
        return s;
    }
}

// ===================================
//   SNAPSHOT PARA JSON (DTO)
// ===================================
public class InventarioSnapshot
{
    public List<Producto> Productos { get; set; }
    public List<Pedido> PedidosFifo { get; set; }      // cola exportada como lista
    public List<Lote> LotesBottomToTop { get; set; }   // pila exportada como lista fondo->cima

    public InventarioSnapshot()
    {
        Productos = new List<Producto>();
        PedidosFifo = new List<Pedido>();
        LotesBottomToTop = new List<Lote>();
    }
}

// ===================================
//   LÓGICA DE INVENTARIO
// ===================================
public class InventorySystem
{
    private Dictionary<int, Producto> _productos;
    private AnotherQueue<Pedido> _pedidos; // Queue -> FIFO
    private AnotherStack<Lote> _lotes;     // Stack -> LIFO

    private string _archivo;

    public InventorySystem(string archivoJson)
    {
        _archivo = archivoJson;
        _productos = new Dictionary<int, Producto>();
        _pedidos = new AnotherQueue<Pedido>(64);
        _lotes = new AnotherStack<Lote>(64);

        CargarDeArchivo(); // intenta cargar; si no existe, crea datos ejemplo
    }

    // --------- Operaciones requeridas por la práctica ---------

    // Registrar un nuevo pedido (Queue)
    public void RegistrarPedido(int productId, int cantidad)
    {
        if (!_productos.ContainsKey(productId))
        {
            throw new Exception("El producto no existe.");
        }
        if (cantidad <= 0)
        {
            throw new Exception("La cantidad debe ser mayor que cero.");
        }

        Pedido p = new Pedido(productId, cantidad);
        _pedidos.Enqueue(p);
        GuardarEnArchivo();
        Console.WriteLine("Pedido encolado.");
    }

    // Procesar el próximo pedido (Queue)
    public void ProcesarProximoPedido()
    {
        if (_pedidos.Count == 0)
        {
            Console.WriteLine("No hay pedidos en cola.");
            return;
        }

        Pedido siguiente = _pedidos.Peek(); // ver sin quitar

        Producto prod;
        if (!_productos.TryGetValue(siguiente.ProductId, out prod))
        {
            Console.WriteLine("El producto del pedido ya no existe. Se eliminará de la cola.");
            _pedidos.Dequeue();
            GuardarEnArchivo();
            return;
        }

        if (prod.Stock >= siguiente.Cantidad)
        {
            _pedidos.Dequeue(); // ahora sí quitar
            prod.Stock = prod.Stock - siguiente.Cantidad;
            GuardarEnArchivo();
            Console.WriteLine("Pedido procesado: " + siguiente.ToString());
            Console.WriteLine("Stock restante de " + prod.Nombre + ": " + prod.Stock);
        }
        else
        {
            Console.WriteLine("Stock insuficiente. El pedido queda en espera al frente de la cola.");
        }
    }

    // Recibir mercancía (Stack)
    public void RecibirMercancia(int productId, int unidades)
    {
        if (!_productos.ContainsKey(productId))
        {
            throw new Exception("El producto no existe.");
        }
        if (unidades <= 0)
        {
            throw new Exception("Las unidades deben ser > 0.");
        }

        Lote lote = new Lote(productId, unidades);
        _lotes.Push(lote);
        GuardarEnArchivo();
        Console.WriteLine("Lote recibido y apilado (Push).");
    }

    // Reabastecer inventario (Stack)
    public void ReabastecerDesdeCima()
    {
        if (_lotes.Count == 0)
        {
            Console.WriteLine("No hay lotes en la pila.");
            return;
        }

        Lote top = _lotes.Pop();
        Producto prod;
        if (_productos.TryGetValue(top.ProductId, out prod))
        {
            prod.Stock = prod.Stock + top.Unidades;
            GuardarEnArchivo();
            Console.WriteLine("Reabastecido +" + top.Unidades + " a " + prod.Nombre + ". Stock actual: " + prod.Stock);
        }
        else
        {
            Console.WriteLine("El lote corresponde a un producto inexistente. (Se descarta).");
            GuardarEnArchivo();
        }
    }

    // --------- Utilidades de visualización ---------
    public void MostrarProductos()
    {
        Console.WriteLine("\n== INVENTARIO ==");
        foreach (Producto p in _productos.Values)
        {
            Console.WriteLine(p.ToString());
        }
        Console.WriteLine();
    }

    public void VerSiguientePedido()
    {
        if (_pedidos.Count == 0)
        {
            Console.WriteLine("No hay pedidos.");
            return;
        }
        Console.WriteLine("Siguiente en cola: " + _pedidos.Peek().ToString());
    }

    public void VerLoteEnCima()
    {
        if (_lotes.Count == 0)
        {
            Console.WriteLine("No hay lotes.");
            return;
        }
        Console.WriteLine("Lote en cima: " + _lotes.Peek().ToString());
    }

    public void AgregarOActualizarProducto(Producto p)
    {
        _productos[p.Id] = p;
        GuardarEnArchivo();
    }

    // =========================
    //   PERSISTENCIA A JSON
    // =========================
    private void GuardarEnArchivo()
    {
        InventarioSnapshot snap = new InventarioSnapshot();

        // Productos
        foreach (Producto p in _productos.Values)
        {
            snap.Productos.Add(new Producto(p.Id, p.Nombre, p.Stock));
        }

        // Pedidos como lista FIFO
        snap.PedidosFifo = _pedidos.ToList();

        // Lotes como lista fondo->cima
        snap.LotesBottomToTop = _lotes.ToListBottomToTop();

        JsonSerializerOptions opts = new JsonSerializerOptions();
        opts.WriteIndented = true;

        string json = JsonSerializer.Serialize<InventarioSnapshot>(snap, opts);
        File.WriteAllText(_archivo, json);
    }

    private void CargarDeArchivo()
    {
        if (!File.Exists(_archivo))
        {
            // Si no existe, creamos algunos productos de ejemplo y guardamos
            _productos[1] = new Producto(1, "Manzanas", 10);
            _productos[2] = new Producto(2, "Naranjas", 5);
            _productos[3] = new Producto(3, "Peras", 0);
            GuardarEnArchivo();
            return;
        }

        string json = File.ReadAllText(_archivo);
        if (string.IsNullOrWhiteSpace(json))
        {
            _productos.Clear();
            _pedidos = new AnotherQueue<Pedido>(64);
            _lotes = new AnotherStack<Lote>(64);
            return;
        }

        InventarioSnapshot snap = JsonSerializer.Deserialize<InventarioSnapshot>(json);
        if (snap == null)
        {
            _productos.Clear();
            _pedidos = new AnotherQueue<Pedido>(64);
            _lotes = new AnotherStack<Lote>(64);
            return;
        }

        // Reconstruir productos
        _productos.Clear();
        if (snap.Productos != null)
        {
            for (int i = 0; i < snap.Productos.Count; i++)
            {
                Producto p = snap.Productos[i];
                _productos[p.Id] = new Producto(p.Id, p.Nombre, p.Stock);
            }
        }

        // Reconstruir cola de pedidos (FIFO)
        _pedidos = AnotherQueue<Pedido>.FromList(snap.PedidosFifo, 16);

        // Reconstruir pila de lotes (fondo->cima)
        _lotes = AnotherStack<Lote>.FromListBottomToTop(snap.LotesBottomToTop, 16);
    }
}

// =======================
//   PROGRAMA CON MENÚ
// =======================
public class Program
{
    public static void Main()
    {
        string archivo = "inventario.json";
        InventorySystem sys = new InventorySystem(archivo);

        while (true)
        {
            Console.WriteLine("====== PRACTICA 2: Pilas (Stack) y Colas (Queue) ======");
            Console.WriteLine("1) Registrar nuevo pedido (Queue -> Enqueue)");
            Console.WriteLine("2) Procesar próximo pedido (Queue -> Peek/Dequeue)");
            Console.WriteLine("3) Recibir mercancía (Stack -> Push)");
            Console.WriteLine("4) Reabastecer inventario (Stack -> Pop)");
            Console.WriteLine("5) Ver inventario");
            Console.WriteLine("6) Ver siguiente pedido (Peek Queue)");
            Console.WriteLine("7) Ver lote en cima (Peek Stack)");
            Console.WriteLine("8) Agregar/Actualizar producto");
            Console.WriteLine("0) Salir");
            Console.Write("Opción: ");

            string op = Console.ReadLine();
            Console.WriteLine();

            try
            {
                if (op == "1")
                {
                    Console.Write("ID de producto: ");
                    int pid = int.Parse(Console.ReadLine());

                    Console.Write("Cantidad: ");
                    int qty = int.Parse(Console.ReadLine());

                    sys.RegistrarPedido(pid, qty);
                }
                else if (op == "2")
                {
                    sys.ProcesarProximoPedido();
                }
                else if (op == "3")
                {
                    Console.Write("ID de producto: ");
                    int bid = int.Parse(Console.ReadLine());

                    Console.Write("Unidades recibidas en el lote: ");
                    int units = int.Parse(Console.ReadLine());

                    sys.RecibirMercancia(bid, units);
                }
                else if (op == "4")
                {
                    sys.ReabastecerDesdeCima();
                }
                else if (op == "5")
                {
                    sys.MostrarProductos();
                }
                else if (op == "6")
                {
                    sys.VerSiguientePedido();
                }
                else if (op == "7")
                {
                    sys.VerLoteEnCima();
                }
                else if (op == "8")
                {
                    Console.Write("ID: ");
                    int id = int.Parse(Console.ReadLine());

                    Console.Write("Nombre: ");
                    string nombre = Console.ReadLine();

                    Console.Write("Stock: ");
                    int stock = int.Parse(Console.ReadLine());

                    Producto p = new Producto(id, nombre, stock);
                    sys.AgregarOActualizarProducto(p);
                    Console.WriteLine("Producto agregado/actualizado.");
                }
                else if (op == "0")
                {
                    Console.WriteLine("Adiós.");
                    return;
                }
                else
                {
                    Console.WriteLine("Opción no válida.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine();
        }
    }
}
