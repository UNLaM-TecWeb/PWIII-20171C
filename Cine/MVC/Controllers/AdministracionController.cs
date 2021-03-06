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
        ManejoCarteleras servicioCarteleras = new ManejoCarteleras();
        ManejoReserva servicioReservas = new ManejoReserva();

        
        public ActionResult Home()
        {
            if (Session["IdUsuario"] == null)
            {
                TempData["urlController"] = Request.RequestContext.RouteData.Values["controller"].ToString();
                TempData["urlAction"] = Request.RequestContext.RouteData.Values["action"].ToString();
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        /* CARTELERAS */
        public ActionResult Carteleras()
        {
            if (Session["IdUsuario"] == null)
            {
                TempData["urlController"] = Request.RequestContext.RouteData.Values["controller"].ToString();
                TempData["urlAction"] = Request.RequestContext.RouteData.Values["action"].ToString();
                return RedirectToAction("Login", "Home");
            }
            
            List<Carteleras> Carteleras = servicioCarteleras.TraerCarteleras(); // Traigo todas las carteleras
            
            List<InfoCarteleras> infoCarteleras = new List<InfoCarteleras>(); // Instancio la clase que utilizo para mostrar las carteleras (y no mostrar ids)

            foreach(Carteleras cartelera in Carteleras)
            {
                InfoCarteleras info = new InfoCarteleras();
                info.IdCartelera = cartelera.IdCartelera;
                info.Sede = servicioSedes.TraerSede(cartelera.IdSede).Nombre;
                info.Sala = cartelera.NumeroSala;
                info.Pelicula = servicioPeliculas.TraerPelicula(cartelera.IdPelicula).Nombre;
                info.Version = servicioReservas.TraerVersion(cartelera.IdVersion).Nombre;
                info.FechaInicio = cartelera.FechaInicio;
                info.FechaFin = cartelera.FechaFin;
                infoCarteleras.Add(info);
            }

            return View(infoCarteleras);
        }

        public ActionResult EliminarCartelera(int id)
        {
            servicioCarteleras.BorrarCartelera(id);
            return RedirectToAction("Carteleras", "Administracion");
        }

        public ActionResult NuevaCartelera()
        {
            ViewBag.Sedes = servicioSedes.TraerSedes(); // Traigo todas las sedes
            ViewBag.Peliculas = servicioPeliculas.TraerPeliculas(); // Traigo todas las peliculas
            ViewBag.Versiones = servicioPeliculas.TraerVersiones(); // Traigo todas las versiones

            return View();
        }       
        
        [HttpPost]
        public ActionResult NuevaCartelera(Carteleras cart)
        {
            // Valido que se haya marcado al menos un dia de la semana
            if (!(servicioCarteleras.DiaValidoCartelera(cart)))
            {
                // Si entra es por que no fue marcado ningun dia
                TempData["Error"] = "Debes seleccionar al menos un dia de la semana";
                return RedirectToAction("Carteleras", "Administracion");
            }
            
            // Valido que la Cartelera no se pise con ninguna otra en esa misma sede y sala
            if (servicioCarteleras.ValidarCartelera(cart))
            {
                // En caso de que la cartelera sea totalmente valida le asigno una fecha de carga y la guardo
                cart.FechaCarga = DateTime.Now;
                servicioCarteleras.GuardarCartelera(cart);
            }
            else
            {
                TempData["Error"] = "No se puede crear una cartelera en esa fecha";
            }

            return RedirectToAction("Carteleras", "Administracion");
        }

        public ActionResult EditarCartelera(int id)
        {
            ViewBag.Sedes = servicioSedes.TraerSedes(); // Traigo todas las sedes
            ViewBag.Peliculas = servicioPeliculas.TraerPeliculas(); // Traigo todas las peliculas
            ViewBag.Versiones = servicioPeliculas.TraerVersiones(); // Traigo todas las versiones

            DateTime fechaInicio = servicioCarteleras.TraerCartelera(id).FechaInicio;
            DateTime fechaFin = servicioCarteleras.TraerCartelera(id).FechaFin;

            string fechaInicioString = fechaInicio.Year.ToString() + "-" + servicioCarteleras.FormadoMes(fechaInicio) + "-" + servicioCarteleras.FormatoDia(fechaInicio);
            string fechaFinString = fechaFin.Year.ToString() + "-" + servicioCarteleras.FormadoMes(fechaFin) + "-" + servicioCarteleras.FormatoDia(fechaFin);

            ViewBag.fechaInicio = fechaInicioString;
            ViewBag.fechaFin = fechaFinString;

            return View(servicioCarteleras.TraerCartelera(id));
        }

        [HttpPost]
        public ActionResult EditarCartelera(Carteleras c)
        {
            if (!((servicioCarteleras.ActualizarCartelera(c))))
            {
                TempData["Error"] = "No se ha podido actualizar la cartelera. Conflicto con otra Cartelera existente";
            }
            return RedirectToAction("Carteleras", "Administracion");
        }

        /* PELICULAS */
        
        public ActionResult Peliculas()
        {
            if (Session["IdUsuario"] == null)
            {
                TempData["urlController"] = Request.RequestContext.RouteData.Values["controller"].ToString();
                TempData["urlAction"] = Request.RequestContext.RouteData.Values["action"].ToString();
                return RedirectToAction("Login", "Home");
            }
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
                TempData["Error"] = "No se pudo crear la pelicula";
                return RedirectToAction("Peliculas", "Administracion");
            }

            if (Imagen != null)
            {
                var filename = DateTime.Now.Second + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + Path.GetFileName(Imagen.FileName);

                var path = Path.Combine(Server.MapPath("~/Content/Upload"), filename);
                Imagen.SaveAs(path);

                // Le asigno el nombre a la imagen de la pelicula
                p.Imagen = filename;
            }
                else
                {
                    p.Imagen = "sinTapa.png"; 
                }
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
                TempData["Error"] = "No se pudo crear la Sede";
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

        /* REPORTES */
        
        public ActionResult Reportes()
        {
            if (Session["IdUsuario"] == null)
            {
                TempData["urlController"] = Request.RequestContext.RouteData.Values["controller"].ToString();
                TempData["urlAction"] = Request.RequestContext.RouteData.Values["action"].ToString();
                return RedirectToAction("Login", "Home");
            }
            
            ViewBag.Peliculas = servicioPeliculas.TraerPeliculas(); // Traigo todas las peliculas
            
            return View();
        }

        [HttpPost]
        public ActionResult ReporteReserva(InfoReportes r)
        {
            // Verifico que el lapso de tiempo no supere los 30 dias
            if (!(ServicioReportes.LapsoValido(r.FechaInicio, r.FechaFin)))
            {
                TempData["Error"] = "El lapso de tiempo no debe ser mayor a 30 Dias.";
                return RedirectToAction("Reportes", "Administracion");
            }

            // Si el lapso de tiempo es valido, voy a la tabla de reserva y traigo todas las reservas comprendidas en ese lapso de tiempo
            // para esa pelicula
            List<Reservas> listaReservas = servicioReservas.TraerReservasPorPeriodo(r.FechaInicio, r.FechaFin, r.Pelicula);
            
            // Consulto si la lista llego vacia, en ese caso muetro un mensaje en pantalla
            if (listaReservas.Count() == 0)
            {
                TempData["Error"] = "No se encontraron reservas para el Rango de Fecha solicitado";
                return RedirectToAction("Reportes", "Administracion");                
            }

            // Creo una lista con los datos requeridos
            List<InfoReserva> listaInfoReserva = new List<InfoReserva>();

            foreach (Reservas reserva in listaReservas)
            {
                InfoReserva infoReserva = new InfoReserva();
                infoReserva.Sede = servicioSedes.TraerSede(reserva.IdSede).Nombre;
                infoReserva.Precio = servicioSedes.TraerSede(reserva.IdSede).PrecioGeneral;
                infoReserva.Pelicula = servicioPeliculas.TraerPelicula(reserva.IdPelicula).Nombre;
                infoReserva.Version = servicioReservas.TraerVersion(reserva.IdVersion).Nombre;
                listaInfoReserva.Add(infoReserva);
            }

            return View(listaInfoReserva);
        }

        /* SEDES */
        public ActionResult Sedes()
        {
            if (Session["IdUsuario"] == null)
            {
                TempData["urlController"] = Request.RequestContext.RouteData.Values["controller"].ToString();
                TempData["urlAction"] = Request.RequestContext.RouteData.Values["action"].ToString();
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Sedes = servicioSedes.TraerSedes(); // Traigo las Sedes de la Base de Datos
            return View();
        }

        [HttpPost]
        public ActionResult Sedes(Sedes s)
        {
            if (!(ModelState.IsValid))
            {
                TempData["Error"] = "No se pudo crear la Sede";
                return RedirectToAction("Sedes", "Administracion");
            }
            
            servicioSedes.GuardarSede(s);

            return RedirectToAction("Sedes", "Administracion");
        }

        public ActionResult EditarSede(int id)
        {
            ViewBag.sede = servicioSedes.TraerSede(id);
            ViewBag.sede.PrecioGeneral = Convert.ToInt32(ViewBag.sede.PrecioGeneral);
            
            return View();
        }

        [HttpPost]
        public ActionResult EditarSede(Sedes s)
        {

            
            if (!(ModelState.IsValid))
            {
                TempData["Error"] = "No se pudo modificar la Sede";
                return RedirectToAction("Sedes", "Administracion");
            }

            servicioSedes.ActualizarSede(s);
            return RedirectToAction("Sedes", "Administracion");
        }

    }
}
