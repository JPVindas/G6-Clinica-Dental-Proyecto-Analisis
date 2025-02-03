document.addEventListener("DOMContentLoaded", function () {
    const addToCartButtons = document.querySelectorAll(".add-to-cart");
    const cartBadge = document.querySelector(".badge");

    let cart = JSON.parse(localStorage.getItem("cart")) || {};

    function updateCartBadge() {
        const itemCount = Object.keys(cart).reduce((total, key) => total + cart[key].quantity, 0);
        cartBadge.textContent = itemCount;
    }

    function addToCart(productId, productName, productPrice, productImage) {
        if (cart[productId]) {
            cart[productId].quantity += 1;
        } else {
            cart[productId] = {
                name: productName,
                price: productPrice,
                quantity: 1,
                image: productImage 
            };
        }
        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartBadge();
    }

    addToCartButtons.forEach((button) => {
        button.addEventListener("click", (event) => {
            event.preventDefault();
            const productId = button.dataset.id;
            const productName = button.dataset.name;
            const productPrice = parseFloat(button.dataset.price);
            const productImage = button.dataset.image; 
            addToCart(productId, productName, productPrice, productImage); 
            alert(`Producto "${productName}" agregado al carrito.`);
        });
    });

    updateCartBadge();
});
