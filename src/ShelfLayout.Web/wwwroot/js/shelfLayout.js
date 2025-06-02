// Define the function in the global scope
window.scrollToElement = function(element) {
    console.log('scrollToElement called with element:', element);
    if (element) {
        console.log('Scrolling to element');
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    } else {
        console.log('No element provided to scroll to');
    }
};

// Log that the script has loaded
console.log('shelfLayout.js loaded'); 