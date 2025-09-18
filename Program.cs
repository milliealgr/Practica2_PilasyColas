using System;
using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        var pedidos = new MyQueue<string>();
        var lotes = new MyStack<string>();
        int stock = 0;

        while (true)
        {
            Console.WriteLine("MENÚ");
            Console.WriteLine("1- Registrar pedido");
            Console.WriteLine("2- Procesar pedido");
            Console.WriteLine("3- Recibir mercancía");
            Console.WriteLine("4- Reabastecer inventario");
            Console.WriteLine("5- Ver stock");
            Console.WriteLine("0- Salir");

            string opcion = Console.ReadLine();

            try
            {
                switch (opcion)
                {
                    case "1":
                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Ingresa una opción válida");
                        break;
                }
            }
            catch
            {
                throw new Exception("Ingresa una opción válida");
            }
        }
    }
}