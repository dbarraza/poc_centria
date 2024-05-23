# ¿Qué hace Centria?

Centria es una empresa de servicios administrativos enfocada en identificar sinergias y generar eficiencias, con el propósito de dar soporte y soluciones para la creación de valor.

# Problematica

En el proceso actual de selección de candidatos para los puestos de trabajo, Centria enfrenta varios desafíos que hacen que el proceso sea lento y tedioso. Actualmente, se recibe un promedio de 8 postulaciones mensuales, **con alrededor de 200 candidatos por cada proceso de selección.** Este volumen de candidatos representa una carga significativa para el equipo de recursos humanos, que debe revisar manualmente cada solicitud, evaluar las cualificaciones de los candidatos y la revisión de los CV de los preseleccionados, para poder encontrar el candito más idóneo al cargo.

Además, el proceso de selección manual es propenso a sesgos y errores humanos, lo que puede resultar en decisiones de contratación subóptimas. Es crucial encontrar formas de optimizar y mejorar el proceso de selección de candidatos para garantizar la selección de los mejores talentos de manera eficiente y efectiva.

# Solución Esperada

Para abordar los desafíos identificados en el proceso de selección de candidatos, proponemos implementar una solución basada en un modelo RAG (Retrieval-Augmented Generative), que combinará la generación de texto con capacidades de recuperación de información para optimizar el proceso de selección y reducir los costos operacionales asociados. **Esta POC permitirá a los usuarios hacer preguntas sobre los candidatos seleccionados y recibir respuestas generadas automáticamente que sean informativas, relevantes y coherentes.**

## La solución consta de 2 grandes componetnes

**Prefiltro de Candidatos**
- Este prefiltro permitirá reducir la carga de trabajo del equipo de recursos humanos al centrarse en los candidatos más calificados y relevantes para el puesto de trabajo.

**Procesamiento de CVs**
- La solución automatizará el procesamiento de los currículums vitae (CVs) de los candidatos preseleccionados, guardando y organizando la información.

# Prueba de Concepto a Implementar

Para validar la efectividad de la solución propuesta, llevaremos a cabo una Prueba de Concepto (POC) que constará de las siguientes etapas:

1. Prefiltro de Candidatos
  - Funcionalidad de prefiltro de candidatos que utilizará criterios objetivos y relevantes para seleccionar automáticamente los 20 candidatos más prometedores de la base de candidatos.
  - Esta funcionalidad reducirá la carga de trabajo del equipo de recursos humanos al centrarse en los candidatos más calificados y relevantes para el puesto de trabajo.
2. Recolección de Datos de Candidatos
  - Se recopilarán los currículums vitae (CVs) de una muestra representativa de candidatos de la base de datos existente (planilla Excel) que contiene la información de los candidatos.
3. Procesamiento Automatizado de CVs
  - Procesar los CVs de los 20 candidatos preseleccionados.
  - El sistema leerá la información de los CVs, la guardará en una base de datos y la organizará para su fácil acceso.
4. Interfaz de Usuario para Consultas
  - Desarrollaremos una interfaz de usuario intuitiva donde los usuarios puedan hacer preguntas sobre los candidatos preseleccionados.
  - Esta interfaz enviará las consultas al modelo RAG y mostrará las respuestas generadas de manera clara y comprensible.
  