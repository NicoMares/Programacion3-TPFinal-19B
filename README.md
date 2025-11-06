# 💻 Proyecto Final: Call Center Web (Programación 3)

Este es el repositorio del Trabajo Práctico Final para la asignatura Programación 3. Se trata de una aplicación web para la gestión de un **Call Center**.

## 🚀 Tecnologías Utilizadas

El proyecto fue desarrollado utilizando el ecosistema de Microsoft, empleando una arquitectura tradicional de tres capas (Dominio, Negocio y Web).

* **Lenguaje:** C#
* **Framework:** ASP.NET WebForms (Aplicación Web de .NET Framework)
* **Patrón de Diseño:** Arquitectura de N capas (Domain, Business, Web)
* **Base de Datos:** SQL Server 

## 📁 Estructura del Proyecto

La solución `CallCenter.sln` se compone de los siguientes proyectos principales:

| Carpeta/Proyecto | Descripción |
| :--- | :--- |
| `CallCenter.Web` | Contiene la interfaz de usuario (UI) en **ASP.NET WebForms**. Aquí se encuentran todos los archivos `.aspx` y el código *Code Behind* que maneja la presentación y la interacción del usuario. |
| `CallCenter.Business` | La **Capa de Negocio** (BLL). Contiene la lógica de la aplicación, como validaciones, procesamiento de datos y la orquestación de operaciones entre la interfaz y la capa de dominio/datos. |
| `CallCenter.Domain` | La **Capa de Dominio/Entidades**. Contiene los modelos (clases) que representan los objetos y la estructura de datos del sistema (por ejemplo, Agente, Cliente, Llamada, etc.). |
| `Backup` | Contiene copias de seguridad u otros archivos no esenciales para la ejecución del código. |

## 🛠️ Requisitos e Instalación

Para levantar y ejecutar este proyecto localmente, necesitas el siguiente entorno de desarrollo:

### Requisitos

1.  **Visual Studio:** Se recomienda **Visual Studio 2019 o superior** con la carga de trabajo "Desarrollo web y ASP.NET".
2.  **SDK de .NET Framework:** 4.x.
3.  **SQL Server:** Una instancia local para alojar la base de datos del sistema.

### Pasos para la Ejecución

1.  **Clonar el Repositorio:**
    ```bash
    git clone [https://github.com/NicoMares/Programacion3-TPFinal-19B.git](https://github.com/NicoMares/Programacion3-TPFinal-19B.git)
    ```
2.  **Abrir la Solución:** Abre el archivo `CallCenter.sln` en Visual Studio.
3.  **Configurar la Base de Datos:**
    * Crea la base de datos y sus tablas en tu instancia de SQL Server.
    * Actualiza la cadena de conexión en el archivo `Web.config` dentro del proyecto `CallCenter.Web` para que apunte a tu base de datos local.
4.  **Restaurar Paquetes NuGet:** Asegúrate de que todos los paquetes de las dependencias estén instalados. Visual Studio generalmente lo hace automáticamente.
5.  **Ejecutar:** Establece el proyecto `CallCenter.Web` como proyecto de inicio y presiona **F5** para iniciar la aplicación.

---

## 👥 Contribuidores

Este proyecto fue desarrollado por:

* **Nicolas Mares** ([@NicoMares](https://github.com/NicoMares))
* **Alejandro Ledesma** ([@aledesma93](https://github.com/aledesma93))
