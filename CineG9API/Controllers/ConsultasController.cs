using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CineG9API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasController : ControllerBase
    {
        // ✅ Ajustá este valor según tu servidor
        private readonly string _connectionString =
            "Data Source=BOSGAME-5OPCE\\SQLEXPRESS;Initial Catalog=BDIG9CINE;Integrated Security=True;TrustServerCertificate=True;";

        [HttpGet("{tipo}")]
        public IActionResult GetConsulta(string tipo)
        {
            // ✅ Bloque opcional para verificar si la conexión funciona
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    Console.WriteLine("✅ Conexión exitosa!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error de conexión: " + ex.Message);
                return StatusCode(500, "Error de conexión con la base de datos: " + ex.Message);
            }

            // ✅ Switch con las consultas
            string query = tipo switch
            {
                // 🔹 Consulta 1 – Quinteros Tomás – 412077
                "quinteros" => @"
                    SELECT
                        'Membresía Costosa' AS Membresia,
                        c.id_cliente,
                        CONCAT(c.apellido, ', ', c.nombre) AS Cliente,
                        m.nombre AS NombreMembresía,
                        m.precio AS PrecioMembresía,
                        sp.fecha_pago AS FechaPago,
                        MONTH(sp.fecha_pago) AS MesPago
                    FROM clientes c
                    JOIN cliente_membresia cm ON cm.id_cliente = c.id_cliente
                    JOIN membresias m ON m.id_membresia = cm.id_membresia
                    JOIN suscripciones s ON s.id_cliente_membresia = cm.id_cliente_membresia
                    JOIN suscripciones_pagos sp ON sp.id_suscripcion = s.id_suscripcion
                    WHERE m.precio > 6000
                      AND MONTH(sp.fecha_pago) % 2 = 0
                      AND NOT EXISTS (
                          SELECT 1
                          FROM suscripciones s2
                          JOIN suscripciones_pagos sp2 ON sp2.id_suscripcion = s2.id_suscripcion
                          WHERE s2.id_cliente_membresia = cm.id_cliente_membresia
                            AND MONTH(sp2.fecha_pago) % 2 <> 0
                      )
                    UNION
                    SELECT
                        'Membresía Barata' AS Membresia,
                        c.id_cliente,
                        CONCAT(c.apellido, ', ', c.nombre) AS Cliente,
                        m.nombre AS NombreMembresía,
                        m.precio AS PrecioMembresía,
                        sp.fecha_pago AS FechaPago,
                        MONTH(sp.fecha_pago) AS MesPago
                    FROM clientes c
                    JOIN cliente_membresia cm ON cm.id_cliente = c.id_cliente
                    JOIN membresias m ON m.id_membresia = cm.id_membresia
                    JOIN suscripciones s ON s.id_cliente_membresia = cm.id_cliente_membresia
                    JOIN suscripciones_pagos sp ON sp.id_suscripcion = s.id_suscripcion
                    WHERE m.precio <= 6000
                      AND MONTH(sp.fecha_pago) % 2 = 0
                      AND NOT EXISTS (
                          SELECT 1
                          FROM suscripciones s2
                          JOIN suscripciones_pagos sp2 ON sp2.id_suscripcion = s2.id_suscripcion
                          WHERE s2.id_cliente_membresia = cm.id_cliente_membresia
                            AND MONTH(sp2.fecha_pago) % 2 <> 0
                      )
                    ORDER BY Membresia, Cliente;",

                // 🔹 Consulta 2 – Ferreyra Peuser Baltazar – 421676
                "ferreyra" => @"
                    SELECT 
                        v1.[Genero de la pelicula],
                        v1.[Recaudacion total],
                        v1.[Total de tickets vendidos]
                    FROM (
                        SELECT 
                            g.descripcion AS [Genero de la pelicula],
                            COUNT(t.id_ticket) AS [Total de tickets vendidos],
                            COUNT(DISTINCT f.id_funcion) AS [Cantidad de funciones],
                            SUM(t.precio) AS [Recaudacion total]
                        FROM generos g
                        INNER JOIN peliculas_generos pg ON pg.id_genero = g.id_genero
                        INNER JOIN peliculas p ON p.id_pelicula = pg.id_pelicula
                        INNER JOIN funciones f ON f.id_pelicula = p.id_pelicula
                        INNER JOIN tickets t ON t.id_funcion = f.id_funcion
                        WHERE YEAR(f.fecha) = 2025
                        GROUP BY g.descripcion
                    ) v1
                    WHERE [Recaudacion total] > (SELECT AVG([Recaudacion total]) FROM (
                        SELECT SUM(t.precio) AS [Recaudacion total]
                        FROM generos g
                        INNER JOIN peliculas_generos pg ON pg.id_genero = g.id_genero
                        INNER JOIN peliculas p ON p.id_pelicula = pg.id_pelicula
                        INNER JOIN funciones f ON f.id_pelicula = p.id_pelicula
                        INNER JOIN tickets t ON t.id_funcion = f.id_funcion
                        WHERE YEAR(f.fecha) = 2025
                        GROUP BY g.descripcion
                    ) v2)
                    ORDER BY [Recaudacion total] DESC;",

                // 🔹 Consulta 3 – Flores Diego – 412317
                "flores" => @"
                    SELECT TOP 5 
                        c.id_cliente, 
                        CONCAT(c.apellido, ' ', c.nombre) AS Cliente,
                        YEAR(f.fecha) AS Año,
                        SUM(df.cantidad) AS [Cant. Productos],
                        SUM(df.cantidad * df.precio_unitario) AS [Gasto Total],
                        SUM(df.cantidad * df.precio_unitario) - 
                        (SELECT AVG(SUM(df2.cantidad * df2.precio_unitario)) 
                         FROM facturas f2 
                         JOIN detalles_facturas df2 ON df2.id_factura = f2.id_factura
                         WHERE YEAR(f2.fecha) = YEAR(GETDATE())
                         GROUP BY f2.id_cliente) AS Diferencia
                    FROM clientes c
                    JOIN facturas f ON f.id_cliente = c.id_cliente
                    JOIN detalles_facturas df ON df.id_factura = f.id_factura
                    WHERE YEAR(f.fecha) = YEAR(GETDATE())
                      AND c.id_cliente NOT IN (SELECT id_cliente FROM clientes_membresia)
                      AND c.id_cliente NOT IN (
                        SELECT f2.id_cliente FROM facturas f2 WHERE YEAR(f2.fecha) < YEAR(GETDATE())
                      )
                    GROUP BY c.id_cliente, c.apellido, c.nombre, YEAR(f.fecha)
                    ORDER BY [Cant. Productos] DESC;",

                // 🔹 Consulta 4 – Dávila Carmen – 412078
                "davila" => @"
                    SELECT 
                        p.titulo,  
                        p.estado,   
                        b.id_bloque_horario AS BloqueHorario  
                    FROM peliculas p  
                    JOIN funciones f ON f.id_pelicula = p.id_pelicula  
                    JOIN bloques_horarios b ON b.id_bloque_horario = f.id_bloque_horario  
                    WHERE MONTH(f.fecha) = MONTH(GETDATE())  
                      AND p.duracion_minutos > 120  
                      AND f.id_bloque_horario = (  
                          SELECT TOP 1 f2.id_bloque_horario  
                          FROM funciones f2  
                          WHERE f2.id_pelicula = f.id_pelicula  
                            AND MONTH(f2.fecha) = MONTH(GETDATE())  
                          GROUP BY f2.id_bloque_horario  
                          ORDER BY COUNT(*) DESC  
                      )  
                    GROUP BY p.titulo, p.estado, b.id_bloque_horario;",
                _ => null
            };

            if (query == null)
                return BadRequest("Consulta no válida");

            var resultados = new List<Dictionary<string, object>>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var fila = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                                fila[reader.GetName(i)] = reader.GetValue(i);
                            resultados.Add(fila);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al ejecutar la consulta: " + ex.Message);
            }

            return Ok(resultados);
        }
    }
}
