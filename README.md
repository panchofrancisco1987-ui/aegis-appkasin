# 🔒 Aegis.Governance
Sistema de auditoría criptográfica para cumplimiento EU AI Act Art. 12.
Desarrollado por Francisco Eduardo Saavedra Rojas.

readme_content = """# 🔐 Aegis.Governance

> Sistema de auditoría criptográfica para cumplimiento **EU AI Act Artículo 12**

[![Netlify Status](https://api.netlify.com/api/v1/badges/imaginative-crostata-31541d/deploy-status)](https://imaginative-crostata-31541d.netlify.app)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![AWS KMS](https://img.shields.io/badge/AWS-KMS-FF9900?logo=amazon-aws)](https://aws.amazon.com/kms/)

---

## 📋 Descripción

**Aegis.Governance** es una plataforma de auditoría criptográfica diseñada para ayudar a organizaciones a cumplir con el **Artículo 12 del EU AI Act**, que exige trazabilidad y registro de decisiones de sistemas de IA de alto riesgo.

A diferencia de las soluciones tradicionales que se limitan a generar "papeleo", Aegis proporciona **prueba criptográfica inmutable** de cada decisión tomada por un sistema de IA, firmada con **AWS KMS HSM** (las claves nunca salen del hardware).

---

## 🎯 ¿Para quién es?

- **CISOs** y **Compliance Officers** en fintechs que usan IA para scoring crediticio
- **HealthTech** con diagnóstico asistido por IA
- Cualquier empresa que despliegue **sistemas de IA de alto riesgo** bajo el EU AI Act

---

## ✨ Características Principales

| Característica | Descripción |
|---|---|
| 🔏 **Firma criptográfica HSM** | Cada decisión de IA se firma con AWS KMS HSM. Las claves nunca salen del hardware. |
| 📄 **Reportes automáticos EU AI Act** | Genera reportes de cumplimiento del Art. 12 automáticamente |
| 🔍 **Verificación de terceros** | Auditores externos pueden verificar integridad de logs sin acceder a claves privadas |
| 🩺 **Health checks** | Monitoreo de KMS, SIEM y Base de Datos en tiempo real |
| 🔐 **Autenticación JWT + RBAC** | Roles: Admin, Auditor, Operator |
| 🐳 **Dockerizado** | Despliegue sencillo con Docker Compose |
| 📊 **Observabilidad** | Métricas con Prometheus |

---

## 🏗️ Arquitectura

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   AI System     │────▶│  Aegis.Gateway  │────▶│  AWS KMS HSM    │
│  (Your Model)   │     │  (.NET 8 API)   │     │  (Sign Decision)│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  PostgreSQL     │
                        │  (Audit Trail)  │
                        └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Prometheus     │
                        │  (Metrics)      │
                        └─────────────────┘
```

---

## 🚀 Cómo Funciona

### Paso 1: Tu IA toma una decisión
El sistema de IA emite una decisión (ej: aprobación de crédito, diagnóstico).

### Paso 2: Aegis la firma con AWS KMS HSM
La decisión se hashea (SHA-256) y se firma criptográficamente usando una clave alojada en **AWS KMS HSM**. La clave privada **nunca sale del hardware**.

### Paso 3: Reguladores verifican la integridad criptográficamente
Cualquier auditor puede verificar la firma usando la clave pública, sin necesidad de acceso a la clave privada ni a los datos sensibles originales.

---

## 💰 Precios

| Plan | Precio | Eventos | Características |
|---|---|---|---|
| **Pro** | $299/mes | 100K eventos | Email support |
| **Business** | $999/mes | 1M eventos | SSO/SAML, Priority support |
| **Enterprise** | Custom | Ilimitado | VPC dedicado, CSM |

---

## 🛠️ Stack Tecnológico

- **Backend:** .NET 8
- **Criptografía:** AWS KMS (Hardware Security Module)
- **Base de datos:** PostgreSQL
- **Contenerización:** Docker
- **Observabilidad:** Prometheus
- **Frontend:** HTML/CSS/JS (desplegado en Netlify)

---

## 🏁 Inicio Rápido

### Requisitos
- Docker & Docker Compose
- Cuenta AWS con acceso a KMS
- .NET 8 SDK (para desarrollo)

### 1. Clona el repositorio
```bash
git clone https://github.com/panchofrancisco1987-ui/Aegis.Governance.git
cd Aegis.Governance
```

### 2. Configura variables de entorno
```bash
cp .env.example .env
# Edita .env con tus credenciales AWS
```

### 3. Levanta con Docker Compose
```bash
docker-compose up -d
```

### 4. Accede a la API
```
http://localhost:5000/swagger
```

---

## 📊 Comparativa

| Característica | Aegis.Governance | Credo AI | IBM watsonx.governance |
|---|---|---|---|
| Firma criptográfica HSM | ✅ | ❌ | ❌ |
| Prueba inmutable | ✅ | ❌ | ❌ |
| Verificación de terceros | ✅ | ⚠️ Limitada | ⚠️ Limitada |
| Reportes EU AI Act Art. 12 | ✅ Automático | ✅ Manual | ✅ Manual |
| Open Source core | ✅ | ❌ | ❌ |
| Precio | Desde $299/mes | Custom | Enterprise |

> **Aegis = prueba criptográfica. Los demás = papeleo y checklists.**

---

## 🔐 Seguridad

- **Claves nunca salen del HSM:** AWS KMS garantiza que las claves privadas permanecen en hardware seguro.
- **RBAC:** Control de acceso granular (Admin, Auditor, Operator).
- **JWT:** Tokens firmados con expiración configurable.
- **Audit trail inmutable:** Una vez firmado, un registro no puede ser alterado sin invalidar la firma.

---

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/nueva-funcionalidad`)
3. Commit (`git commit -m 'Add: nueva funcionalidad'`)
4. Push (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto tiene un **núcleo open source** bajo licencia MIT. La versión SaaS incluye características adicionales de gestión y soporte.

---

## 👤 Autor

**Francisco Eduardo Saavedra Rojas**

- 🐙 GitHub: [@panchofrancisco1987-ui](https://github.com/panchofrancisco1987-ui)
- 💼 LinkedIn: [Tu LinkedIn]
- 📧 Email: [Tu email]

---

> 🛡️ **Aegis.Governance** — *Immutable cryptographic proof for AI compliance.*
>
> Built with assistance from [Kimi AI](https://www.moonshot.cn) (Moonshot AI)
"""

with open('/mnt/agents/output/README_Aegis_Governance.md', 'w', encoding='utf-8') as f:
    f.write(readme_content)

print("README guardado correctamente."). 
readme_content = """# 🔐 Aegis.Governance

> Sistema de auditoría criptográfica para cumplimiento **EU AI Act Artículo 12**

[![Netlify Status](https://api.netlify.com/api/v1/badges/imaginative-crostata-31541d/deploy-status)](https://imaginative-crostata-31541d.netlify.app)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![AWS KMS](https://img.shields.io/badge/AWS-KMS-FF9900?logo=amazon-aws)](https://aws.amazon.com/kms/)

---

## 📋 Descripción

**Aegis.Governance** es una plataforma de auditoría criptográfica diseñada para ayudar a organizaciones a cumplir con el **Artículo 12 del EU AI Act**, que exige trazabilidad y registro de decisiones de sistemas de IA de alto riesgo.

A diferencia de las soluciones tradicionales que se limitan a generar "papeleo", Aegis proporciona **prueba criptográfica inmutable** de cada decisión tomada por un sistema de IA, firmada con **AWS KMS HSM** (las claves nunca salen del hardware).

---

## 🎯 ¿Para quién es?

- **CISOs** y **Compliance Officers** en fintechs que usan IA para scoring crediticio
- **HealthTech** con diagnóstico asistido por IA
- Cualquier empresa que despliegue **sistemas de IA de alto riesgo** bajo el EU AI Act

---

## ✨ Características Principales

| Característica | Descripción |
|---|---|
| 🔏 **Firma criptográfica HSM** | Cada decisión de IA se firma con AWS KMS HSM. Las claves nunca salen del hardware. |
| 📄 **Reportes automáticos EU AI Act** | Genera reportes de cumplimiento del Art. 12 automáticamente |
| 🔍 **Verificación de terceros** | Auditores externos pueden verificar integridad de logs sin acceder a claves privadas |
| 🩺 **Health checks** | Monitoreo de KMS, SIEM y Base de Datos en tiempo real |
| 🔐 **Autenticación JWT + RBAC** | Roles: Admin, Auditor, Operator |
| 🐳 **Dockerizado** | Despliegue sencillo con Docker Compose |
| 📊 **Observabilidad** | Métricas con Prometheus |

---

## 🏗️ Arquitectura

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   AI System     │────▶│  Aegis.Gateway  │────▶│  AWS KMS HSM    │
│  (Your Model)   │     │  (.NET 8 API)   │     │  (Sign Decision)│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  PostgreSQL     │
                        │  (Audit Trail)  │
                        └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Prometheus     │
                        │  (Metrics)      │
                        └─────────────────┘
```

---

## 🚀 Cómo Funciona

### Paso 1: Tu IA toma una decisión
El sistema de IA emite una decisión (ej: aprobación de crédito, diagnóstico).

### Paso 2: Aegis la firma con AWS KMS HSM
La decisión se hashea (SHA-256) y se firma criptográficamente usando una clave alojada en **AWS KMS HSM**. La clave privada **nunca sale del hardware**.

### Paso 3: Reguladores verifican la integridad criptográficamente
Cualquier auditor puede verificar la firma usando la clave pública, sin necesidad de acceso a la clave privada ni a los datos sensibles originales.

---

## 💰 Precios

| Plan | Precio | Eventos | Características |
|---|---|---|---|
| **Pro** | $299/mes | 100K eventos | Email support |
| **Business** | $999/mes | 1M eventos | SSO/SAML, Priority support |
| **Enterprise** | Custom | Ilimitado | VPC dedicado, CSM |

---

## 🛠️ Stack Tecnológico

- **Backend:** .NET 8
- **Criptografía:** AWS KMS (Hardware Security Module)
- **Base de datos:** PostgreSQL
- **Contenerización:** Docker
- **Observabilidad:** Prometheus
- **Frontend:** HTML/CSS/JS (desplegado en Netlify)

---

## 🏁 Inicio Rápido

### Requisitos
- Docker & Docker Compose
- Cuenta AWS con acceso a KMS
- .NET 8 SDK (para desarrollo)

### 1. Clona el repositorio
```bash
git clone https://github.com/panchofrancisco1987-ui/Aegis.Governance.git
cd Aegis.Governance
```

### 2. Configura variables de entorno
```bash
cp .env.example .env
# Edita .env con tus credenciales AWS
```

### 3. Levanta con Docker Compose
```bash
docker-compose up -d
```

### 4. Accede a la API
```
http://localhost:5000/swagger
```

---

## 📊 Comparativa

| Característica | Aegis.Governance | Credo AI | IBM watsonx.governance |
|---|---|---|---|
| Firma criptográfica HSM | ✅ | ❌ | ❌ |
| Prueba inmutable | ✅ | ❌ | ❌ |
| Verificación de terceros | ✅ | ⚠️ Limitada | ⚠️ Limitada |
| Reportes EU AI Act Art. 12 | ✅ Automático | ✅ Manual | ✅ Manual |
| Open Source core | ✅ | ❌ | ❌ |
| Precio | Desde $299/mes | Custom | Enterprise |

> **Aegis = prueba criptográfica. Los demás = papeleo y checklists.**

---

## 🔐 Seguridad

- **Claves nunca salen del HSM:** AWS KMS garantiza que las claves privadas permanecen en hardware seguro.
- **RBAC:** Control de acceso granular (Admin, Auditor, Operator).
- **JWT:** Tokens firmados con expiración configurable.
- **Audit trail inmutable:** Una vez firmado, un registro no puede ser alterado sin invalidar la firma.

---

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/nueva-funcionalidad`)
3. Commit (`git commit -m 'Add: nueva funcionalidad'`)
4. Push (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto tiene un **núcleo open source** bajo licencia MIT. La versión SaaS incluye características adicionales de gestión y soporte.

---

## 👤 Autor

**Francisco Eduardo Saavedra Rojas**

- 🐙 GitHub: [@panchofrancisco1987-ui](https://github.com/panchofrancisco1987-ui)
- 💼 LinkedIn: [Tu LinkedIn]
- 📧 Email: [Tu email]

---

> 🛡️ **Aegis.Governance** — *Immutable cryptographic proof for AI compliance.*
>
> Built with assistance from [Kimi AI](https://www.moonshot.cn) (Moonshot AI)
"""

with open('/mnt/agents/output/README_Aegis_Governance.md', 'w', encoding='utf-8') as f:
    f.write(readme_content)

print("README guardado correctamente.")


## Estado
🚧 MVP en desarrollo activo
