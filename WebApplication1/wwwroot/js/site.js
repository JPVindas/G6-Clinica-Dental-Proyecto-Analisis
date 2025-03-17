document.addEventListener("DOMContentLoaded", function () {
    const cartBadge = document.getElementById("cart-badge");
    let isRemovingFromCart = false;

    // ✅ Función para actualizar el contador del carrito en el navbar
    async function updateCartBadge() {
        if (!cartBadge) return;

        try {
            let response = await fetch("/Carrito/Carrito");
            if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

            let contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                console.error("❌ Error: El servidor devolvió HTML en lugar de JSON.");
                return;
            }

            let data = await response.json();
            cartBadge.textContent = data.cantidadCarrito ?? "0";
        } catch (error) {
            console.error("❌ Error al actualizar el carrito:", error);
        }
    }

    updateCartBadge();

    // ✅ Evento delegado para eliminar productos del carrito
    document.body.addEventListener("click", async function (event) {
        const removeBtn = event.target.closest(".remove-from-cart");
        if (removeBtn) {
            event.preventDefault();
            event.stopPropagation();
            await handleRemoveFromCart(removeBtn);
        }
    });

    // ✅ Función eliminar ítem del carrito con animación
    async function removeFromCart(idCarrito, button) {
        if (isRemovingFromCart) return;
        isRemovingFromCart = true;

        button.setAttribute("disabled", "true"); // 🔹 Deshabilitar botón para evitar doble clic

        try {
            let response = await fetch("/Carrito/EliminarItem", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({ idCarrito: idCarrito })
            });

            if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

            await updateCartBadge();

            // 🔹 Animación de eliminación del ítem antes de quitarlo del DOM
            let row = button.closest("tr");
            if (row) {
                row.style.opacity = "0";
                setTimeout(() => row.remove(), 300); // 🔹 Se borra después de la animación
            }
        } catch (error) {
            console.error("❌ Error de conexión al eliminar el producto:", error);
            alert("❌ Hubo un problema al eliminar el producto.");
        } finally {
            isRemovingFromCart = false;
            button.removeAttribute("disabled");
        }
    }

    async function handleRemoveFromCart(button) {
        const idCarrito = button.dataset.id;
        await removeFromCart(idCarrito, button);
    }

    // ✅ Evento único para limpiar carrito sin salir de la vista
    const clearCartButton = document.getElementById("clear-cart");
    if (clearCartButton) {
        clearCartButton.addEventListener("click", clearCart);
    }

    async function clearCart() {
        if (!confirm("🗑️ ¿Estás seguro de vaciar el carrito?")) return;

        try {
            let response = await fetch("/Carrito/VaciarCarrito", {
                method: "POST",
                headers: { "Content-Type": "application/json" }
            });

            if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

            await updateCartBadge();

            // 🔹 Elimina todos los elementos visualmente sin salir de la página
            let tbody = document.querySelector("tbody");
            if (tbody) {
                tbody.innerHTML = `<tr><td colspan="6" class="text-center fw-bold text-danger">No hay productos en el carrito.</td></tr>`;
            }
        } catch (error) {
            console.error("❌ Hubo un problema al vaciar el carrito:", error);
            alert("❌ Hubo un problema al vaciar el carrito.");
        }
    }
});
