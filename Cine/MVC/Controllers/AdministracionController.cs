﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Logica;
using Entidades;
using System.IO;
using MVC.Models;

namespace MVC.Controllers
{
    public class AdministracionController : Controller
    {
        ManejoSedes servicioSedes = new ManejoSedes();
        ManejoPeliculas servicioPeliculas = new ManejoPeliculas();
        ManejoReportes ServicioReportes = new ManejoReportes();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Carteleras()
        {
            ViewBag.Sedes = servicioSedes.TraerSedes();
            ViewBag.Peliculas = servicioPeliculas.TraerPeliculas();
            ViewBag.Versiones = servicioPeliculas.TraerVersiones();
            return View();
        }

        public ActionResult Peliculas()
        {
            ViewBag.Sedes = servicioSedes.TraerSedes();
            return View(servicioPeliculas.TraerPeliculas());
        }

        public ActionResult NuevaPelicula()
        {
            ViewBag.Calificaciones = servicioPeliculas.TraerCalificaciones();
            ViewBag.Sedes = servicioSedes.TraerSedes();
            ViewBag.Generos = servicioPeliculas.TraerGeneros();
            return View();
        }

        [HttpPost]
        public ActionResult NuevaPelicula(Peliculas p, HttpPostedFileBase Imagen)
        {
            if (!(ModelState.IsValid))
            {
                return RedirectToAction("Peliculas", "Administracion");
            }

            var filename = DateTime.Now.Second + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + Path.GetFileName(Imagen.FileName);

            var path = Path.Combine(Server.MapPath("~/Content/Upload"), filename);
            Imagen.SaveAs(path);

            // Le asigno el nombre a la imagen de la pelicula
            p.Imagen = filename;

            // Le asigno una fecha de carga
            p.FechaCarga = DateTime.Now;

            servicioPeliculas.AgregarPelicula(p);
            return RedirectToAction("Peliculas", "Administracion");
        }

        public ActionResult EditarPelicula(int id)
        {
            ViewBag.Generos = servicioPeliculas.TraerGeneros();
            ViewBag.Calificaciones = servicioPeliculas.TraerCalificaciones();
            Peliculas pelicula= servicioPeliculas.TraerPelicula(id);
            TempData["imagen"] = pelicula.Imagen;
            return View(pelicula);
        }

        [HttpPost]
        public ActionResult EditarPelicula(Peliculas p, HttpPostedFileBase Imagen)
        {
            if (!(ModelState.IsValid))
            {
                return RedirectToAction("Peliculas", "Administracion");
            }
            
            var imgVieja = TempData["imagen"];

            if (p.Imagen != imgVieja && p.Imagen != null)
            {
                var filename = DateTime.Now.Second + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + Path.GetFileName(Imagen.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Upload"), filename);
                Imagen.SaveAs(path);
                p.Imagen = filename;
            }
            else
            { p.Imagen = Convert.ToString(imgVieja); }

            servicioPeliculas.ModificarPelicula(p);
            return RedirectToAction("Peliculas", "Administracion");
        }

        public ActionResult Reportes()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reportes(IntervaloDeTiempo inter)
        {
            DateTime Desde = new DateTime(inter.DesdeAnno, inter.DesdeMes, inter.DesdeDia);
            DateTime Hasta = new DateTime(inter.HastaAnno, inter.HastaMes, inter.HastaDia);

            return View("ReporteReservas", ServicioReportes.GenerarReporteReservas(Desde, Hasta));
        }

        public ActionResult Sedes()
        {
            ViewBag.Sedes = servicioSedes.TraerSedes(); // Traigo las Sedes de la Base de Datos
            return View();
        }

        [HttpPost]
        public ActionResult Sedes(Sedes s)
        {
            if (!(ModelState.IsValid))
            {
                return RedirectToAction("Sedes", "Administracion");
            }
            
            servicioSedes.GuardarSede(s);

            return RedirectToAction("Sedes", "Administracion");
        }

        public ActionResult EditarSede(int id)
        {
            ViewBag.sede = servicioSedes.TraerSede(id);
            return View();
        }

        [HttpPost]
        public ActionResult EditarSede(Sedes s)
        {
            if (!(ModelState.IsValid))
            {
                return RedirectToAction("Sedes", "Administracion");
            }

            servicioSedes.ActualizarSede(s);
            return RedirectToAction("Sedes", "Administracion");
        }

    }
}
