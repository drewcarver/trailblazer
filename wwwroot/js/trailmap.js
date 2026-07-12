const trailStyle = {
    color: "#ff3300",  // Bright red-orange line
    weight: 4,         // Thickness of the line
    opacity: 0.85,     // Transparency
    lineJoin: 'round'  // Smooth intersections
};

let trailGeoJSONData, activeMileMarker, map;

function drawPath(startPoint, endPoint) {
    const pathLine = turf.lineString([
        startPoint,
        endPoint
    ]);
    
    // Add the line to the map
    L.geoJSON(pathLine, {
        style: {
            color: '#FFD700',  // Yellow color for path
            weight: 4,
            opacity: 0.8
        }
    }).addTo(map);
}


function getPointAlongMultiLine(trailJson, targetDistance, options = { units: 'miles' }) {
  const flattened = turf.flatten(trailJson);
  const segments = flattened.features;

  let accumulatedDistance = 0;

  for (const segment of segments) {
    const segmentLength = turf.length(segment, options);

    if (accumulatedDistance + segmentLength >= targetDistance) {
      const remainingDistanceNeeded = targetDistance - accumulatedDistance;
      
      return turf.along(segment, remainingDistanceNeeded, options);
    }

    accumulatedDistance += segmentLength;
  }

  const lastSegment = segments[segments.length - 1];
  const lastCoords = lastSegment.geometry.coordinates;
  return turf.point(lastCoords[lastCoords.length - 1]);
}

function addMileMarker(miles) {
    if (!trailGeoJSONData) {
        console.error("Trail data hasn't loaded yet!");
        return;
    }

    if (activeMileMarker) {
        map.removeLayer(activeMileMarker);
    }

    const totalDistance = turf.length(trailGeoJSONData, { units: 'miles' });
    console.log(`Trail length: ${totalDistance} miles`);

    try {
        const targetPoint = getPointAlongMultiLine(trailGeoJSONData, miles)
        
        const coords = targetPoint.geometry.coordinates;
        const leafletLatLng = [coords[1], coords[0]];

        activeMileMarker = L.marker(leafletLatLng)
            .addTo(map)
            .bindPopup(`<b>Mile Marker: ${miles}</b><br>Appalachian Trail Path Location`)
            .openPopup();

        map.panTo(leafletLatLng);

        return leafletLatLng
    } catch (error) {
        console.error("Could not calculate mile marker. Check your GeoJSON structure:", error);
    }
}


document.addEventListener('DOMContentLoaded', function () {
    const startPointSelect = document.querySelector('#start-point-select')
    const endPointSelect = document.querySelector('#end-point-select')
    
    let startPoint, endPoint;

    startPointSelect.addEventListener('change', e => {
        const selectedOption = startPointSelect.querySelector('option:checked');
        console.log(selectedOption.dataset.mile)
        startPoint = addMileMarker(selectedOption.dataset.mile)
        
        if (endPoint) {
            drawPath(startPoint, endPoint);
        }
    })

    endPointSelect.addEventListener('change', e => {
        const selectedOption = endPointSelect.querySelector('option:checked');
        console.log(selectedOption.dataset.mile)
        endPoint = addMileMarker(selectedOption.dataset.mile)
        
        if (startPoint) {
            drawPath(startPoint, endPoint);
        }
    })

    map = L.map('map').setView([34.628, -84.193], 13); 

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    fetch('/trails/AppalachianTrail2.json')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            trailGeoJSONData = data
            const trailLayer = L.geoJSON(data, {
                style: trailStyle
            }).addTo(map);

            map.fitBounds(trailLayer.getBounds());
        })
        .catch(error => {
            console.error('Error loading the trail GeoJSON:', error);
        });
});