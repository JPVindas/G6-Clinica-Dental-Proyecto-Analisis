# 🦷 Clínica Dental San Rafael  
### 💻 Full-Stack Web Management System | ASP.NET MVC + MySQL  

> *"More than code — a digital system built to organize, connect, and improve lives through technology."* 💙  

---

## 🌟 Overview  

**Clínica Dental San Rafael** is an advanced **web management system** designed to modernize the daily workflow of dental clinics.  
From appointment scheduling 🗓️ and patient management 🧍‍♀️ to billing 💳 and inventory 📦 — everything is automated, efficient, and beautifully integrated.

This project was developed with **ASP.NET MVC 5**, **Entity Framework**, and **MySQL**, applying modern design principles and layered architecture to ensure scalability, security, and maintainability.  

Built with dedication and a clear goal: to make technology useful for people and professionals who take care of smiles. 😄  

---

## ⚙️ Key Features  

### 👥 Multi-Role Access System  
- 👑 **Administrator** – Full control of the system, users, and reports.  
- 👩‍⚕️ **Odontologist** – Manages treatments, clinical histories, and appointments.  
- 🧑‍💼 **Staff / Employee** – Handles inventory, purchases, and patient assistance.  
- 🧍‍♂️ **Patient** – Books appointments, views records, and receives notifications.  

---

### 🗓️ Appointment Management  
- Real-time scheduling with available hours.  
- Validation of clinic hours and buffer time between appointments.  
- Automatic prevention of overlapping or unavailable slots.  
- Calendar view with color-coded status indicators.  

---

### 💳 Billing & Financing  
- Automatic generation of invoices linked to patients, services, and products.  
- Optional discounts and tax calculation per item or service.  
- Financing module for long-term treatment plans.  
- PDF invoice generation and automatic email delivery.  

---

### 🧾 Inventory & Product Control  
- Tracks product quantities and low-stock alerts.  
- Categorized products with CRUD functionality.  
- Integration with the billing module for automatic stock updates.  

---

### 📬 Email & Notifications  
- Email confirmation for registrations, appointments, and purchases.  
- SMTP configuration using dependency injection.  
- Dynamic templates with patient and appointment data.  

---

### 🧠 Database Automation (Triggers & Procedures)  
- Automatic patient creation when a new user with role “Patient” is added.  
- Auto-registration of employees when created by the admin.  
- Financing records auto-generated upon treatment registration.  
- Cascading deletions handled with integrity-preserving procedures.  

---

## 🧩 Tech Stack  

| Layer | Technologies |
|-------|---------------|
| **Frontend** | HTML5, CSS3, Bootstrap, Razor Views |
| **Backend** | C#, ASP.NET MVC 5, Entity Framework |
| **Database** | MySQL (Workbench 8.0) |
| **Tools** | Visual Studio 2022, Git, X.PagedList |
| **Security** | BCrypt password encryption, role-based authentication |
| **Cloud** | Azure Deployment, SMTP Email Integration |

---

## 🏗️ System Architecture  

The system follows a **layered architecture**:

