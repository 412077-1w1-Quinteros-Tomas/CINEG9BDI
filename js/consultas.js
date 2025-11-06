const enunciados = {
  quinteros: `Mostrar en un único listado los clientes diferenciados en dos grupos, según el costo de su membresía:
- 'Membresía Costosa' para los que pagan más de $6000.
- 'Membresía Barata' para los que pagan menos de $6000.
Solo se incluyen clientes con suscripciones pagadas en meses pares.`,
  
  ferreyra: `Parte A: Crear una vista que muestre para cada género la cantidad total de tickets vendidos, funciones y recaudación del año 2025.
Parte B: Mostrar los géneros cuya recaudación supere el promedio, con descripción, recaudación y cantidad de espectadores.`,
  
  flores: `Mostrar los clientes que más productos o entradas compraron este año, que nunca compraron en años anteriores y nunca tuvieron membresía. Mostrar el total gastado y la diferencia con el promedio general.`,
  
  davila: `Obtener el nombre y estado de las películas que tuvieron los bloques de horario más frecuentes este mes, considerando solo las de más de 2 horas de duración.`
};

function mostrarEnunciado() {
  const seleccion = document.getElementById("consultaSelect").value;
  document.getElementById("enunciado").innerText = enunciados[seleccion] || "";
}

async function consultar() {
  const tipo = document.getElementById("consultaSelect").value;
  if (!tipo) {
    alert("Seleccioná una consulta primero");
    return;
  }

  const tablaHeader = document.getElementById("tablaHeader");
  const tablaBody = document.getElementById("tablaBody");
  tablaHeader.innerHTML = "";
  tablaBody.innerHTML = "";

  try {
    const res = await fetch("http://localhost:5132/api/consultas/" + tipo);
    if (!res.ok) throw new Error("Error al obtener datos");
    const data = await res.json();

    if (data.length === 0) {
      tablaBody.innerHTML = `<tr><td colspan="10">Sin resultados</td></tr>`;
      return;
    }

    const columnas = Object.keys(data[0]);
    tablaHeader.innerHTML = `<tr>${columnas.map(c => `<th>${c}</th>`).join("")}</tr>`;
    data.forEach(fila => {
      tablaBody.innerHTML += `<tr>${columnas.map(c => `<td>${fila[c]}</td>`).join("")}</tr>`;
    });

  } catch (err) {
    console.error(err);
    alert("Error al conectar con el servidor");
  }
}
