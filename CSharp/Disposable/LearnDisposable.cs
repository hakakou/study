using System;
using System.IO;

namespace Disposable
{

    public class AnimalSimple
    {
        MemoryStream ms;
        public AnimalSimple(int id)
        {
            Id = id;
            ms = new MemoryStream(new Byte[10000]);
            Console.WriteLine($"allocate any unmanaged resources {Id}");
        }

        ~AnimalSimple()
        {
            Console.WriteLine($"deallocate any unmanaged resources {Id}");
            ms.Dispose();
        }

        public int Id { get; }
    }

    public class AnimalDisposable : IDisposable
    {
        MemoryStream ms;
        public AnimalDisposable(int id)
        {
            Id = id;
            ms = new MemoryStream(new Byte[10000]);
            Console.WriteLine($"allocate any unmanaged resources {Id}");
        }
        ~AnimalDisposable()
        {
            if (!disposed)
                Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        public int Id { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            Console.WriteLine($"deallocate any unmanaged resources {Id}");

            if (disposing)
            {
                ms.Dispose();
                Console.WriteLine($"deallocate any other *managed* resources {Id}");
            }

            disposed = true;
        }

    }

    public class PrototypeDisposable : IDisposable
    {
        public PrototypeDisposable()
        {
            Console.WriteLine("allocate any unmanaged resources");
        }
        ~PrototypeDisposable()
        {
            if (!disposed)
                Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            Console.WriteLine("deallocate any unmanaged resources");

            if (disposing)
            {
                Console.WriteLine("deallocate any other *managed* resources");
            }

            disposed = true;
        }
    }



}