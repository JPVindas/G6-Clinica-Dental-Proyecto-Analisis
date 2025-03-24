document.addEventListener("DOMContentLoaded", function () {
    // Obtiene el badge del carrito y el input oculto con el id del paciente
    const cartBadge = document.getElementById("cart-badge");
    const userIdElement = document.getElementById("user-id");
    let idPaciente = userIdElement ? userIdElement.value.trim() : "";

    if (!idPaciente) {
        console.warn("⚠️ No se pudo detectar el ID del paciente.");
        return;
    }

    // Función para actualizar el badge del carrito
    async function updateCartBadge() {
        if (!cartBadge || !idPaciente) {
            console.warn("⚠️ No se puede actualizar el carrito: ID de paciente no disponible.");
            return;
        }
        try {
            let response = await fetch(`/Carrito/GetCarritoCount?idPaciente=${idPaciente}`);
            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                console.error("❌ Error: El servidor no devolvió JSON.");
                return;
            }
            let data = await response.json();
            cartBadge.textContent = data.cantidadCarrito ?? "0";
            console.log(`🛒 Carrito actualizado: ${data.cantidadCarrito} ítems.`);
        } catch (error) {
            console.error("❌ Error al actualizar el carrito:", error);
        }
    }

    // Función opcional para actualizar un contenedor de ítems (si existe)
    async function updateCartItemsContainer() {
        const cartItemsContainer = document.getElementById("cart-items-container");
        if (!cartItemsContainer) return;
        try {
            let response = await fetch(`/Carrito/GetCartItems?idPaciente=${idPaciente}`);
            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                console.error("❌ Error: El servidor no devolvió JSON en GetCartItems.");
                return;
            }
            let data = await response.json();
            const items = data.items || [];
            // Limpiar contenedor
            cartItemsContainer.innerHTML = "";
            // Recorrer y agregar cada ítem
            items.forEach(item => {
                const subtotal = item.precioUnitario * item.cantidad;
                const totalLinea = subtotal + item.impuestos;
                const itemHtml = `
                    <div class="cart-item d-flex align-items-center mb-2">
                        <img src="${item.imagenUrl ? item.imagenUrl : '/images/default-product.jpg'}" 
                             alt="${item.nombre}" 
                             style="width:50px; height:auto; margin-right:10px;">
                        <div>
                            <strong>${item.nombre}</strong><br />
                            Cantidad: ${item.cantidad} - Total: ₡${totalLinea.toFixed(2)}
                        </div>
                    </div>
                `;
                cartItemsContainer.innerHTML += itemHtml;
            });
        } catch (error) {
            console.error("❌ Error al actualizar los ítems del carrito:", error);
        }
    }

    // Llamamos inicialmente a actualizar el badge y el contenedor (si existe)
    updateCartBadge();
    if (document.getElementById("cart-items-container")) {
        updateCartItemsContainer();
    }

    // Actualizar cada 10 segundos (10000 ms)
    setInterval(updateCartBadge, 10000);
    if (document.getElementById("cart-items-container")) {
        setInterval(updateCartItemsContainer, 10000);
    }
});
