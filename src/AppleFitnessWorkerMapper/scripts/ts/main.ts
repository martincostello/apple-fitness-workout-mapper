const initializer = () => {
    const london = { lat: 51.50809, lng: -0.1285907 };
    const map = new google.maps.Map(document.getElementById('map'), {
        zoom: 14,
        center: london,
    });
    new google.maps.Marker({
        position: london,
        map: map,
    });
};

google.maps.event.addDomListener(window, 'load', initializer);
