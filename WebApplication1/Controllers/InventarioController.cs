﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.DATA;

namespace WebApplication1.Controllers
{
    public class InventarioController : Controller
    {
        private readonly MinombredeconexionDbContext _context;

        // Constructor con inyección de dependencias
        public InventarioController(MinombredeconexionDbContext context)
        {
            _context = context;
        }

        // ✅ FORMULARIO PARA AGREGAR PRODUCTO (GET)
        public IActionResult Agregar()
        {
            return View(new ProductosModel()); // Evita que Model sea null
        }

        // ✅ PROCESAR CREACIÓN DE PRODUCTO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar([Bind("Nombre,Descripcion,Marca,Precio,Stock,UrlImagen")] ProductosModel producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Inventario));
            }
            return View(producto);
        }


        // ✅ LISTAR INVENTARIO
        public async Task<IActionResult> Inventario()
        {
            var productos = await _context.Productos.ToListAsync();
            return View("Inventario", productos);
        }

        // ✅ VER DETALLES DEL PRODUCTO
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View("Detalles", producto);
        }

        // ✅ FORMULARIO PARA EDITAR STOCK (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View("Editar", producto);
        }

        // ✅ PROCESAR EDICIÓN DEL STOCK (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProductosModel productoEditado)
        {
            if (id != productoEditado.Id)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Asegurar que solo se actualiza el stock sin perder otros datos
                    Console.WriteLine($"Stock antes: {producto.Stock}, Stock nuevo: {productoEditado.Stock}");

                    producto.Stock = productoEditado.Stock; // Solo actualiza el stock

                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Stock actualizado correctamente.";
                    return RedirectToAction(nameof(Inventario)); // Redirigir al inventario
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExiste(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View("Editar", productoEditado);
        }



        // ✅ CONFIRMACIÓN PARA ELIMINAR PRODUCTO (GET)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // ✅ PROCESAR ELIMINACIÓN DEL PRODUCTO (POST)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "El producto se eliminó correctamente.";
            return RedirectToAction(nameof(Inventario));
        }

        private bool ProductoExiste(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
