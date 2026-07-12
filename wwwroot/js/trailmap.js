const trailStyle = {
    color: "#ff3300",  // Bright red-orange line
    weight: 4,         // Thickness of the line
    opacity: 0.85,     // Transparency
    lineJoin: 'round'  // Smooth intersections
};

let trailGeoJSONData, activeMileMarker, map;
let hikeLayer;

// FIXED: Slices the trail exactly along its geometry using Turf's slice tools
function drawPath(startMile, endMile) {
    if (!trailGeoJSONData) {
        console.error("Trail data hasn't loaded yet!");
        return;
    }

    if (hikeLayer) {
        map.removeLayer(hikeLayer)
    }

    try {
        // Flatten the geometry down to a single LineString if it's a MultiLineString
        const flattened = turf.flatten(trailGeoJSONData);
        let pathLine;

        if (flattened.features.length === 1) {
            // Standard single continuous line
            pathLine = turf.lineSliceAlong(flattened.features[0], startMile, endMile, { units: 'miles' });
        } else {
            // Complex trail network: fallback to step-by-step feature merging
            let accumulatedDistance = 0;
            let pathCoordinates = [];

            for (const segment of flattened.features) {
                const segmentLength = turf.length(segment, { units: 'miles' });
                const nextDistance = accumulatedDistance + segmentLength;

                // Check if our slicing window falls inside this specific segment
                if (nextDistance >= startMile && accumulatedDistance <= endMile) {
                    const localStart = Math.max(0, startMile - accumulatedDistance);
                    const localEnd = Math.min(segmentLength, endMile - accumulatedDistance);
                    
                    const slicedSegment = turf.lineSliceAlong(segment, localStart, localEnd, { units: 'miles' });
                    pathCoordinates.push(...slicedSegment.geometry.coordinates);
                }
                accumulatedDistance = nextDistance;
            }
            pathLine = turf.lineString(pathCoordinates);
        }

        // Add the slice to the map
        hikeLayer = L.geoJSON(pathLine, {
            style: {
                color: '#FFD700',  // High-visibility gold/yellow
                weight: 6,         // Thick enough to overlap the trail base
                opacity: 0.9,
                lineJoin: 'round'
            }
        }).addTo(map);

        map.fitBounds(hikeLayer.getBounds());
    } catch (error) {
        console.error("Error slicing or drawing the path line:", error);
    }
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

    try {
        const targetPoint = getPointAlongMultiLine(trailGeoJSONData, miles);
        const coords = targetPoint.geometry.coordinates;
        
        // Correct Leaflet coordinate parsing order [lat, lng]
        const leafletLatLng = [coords[1], coords[0]];

        activeMileMarker = L.marker(leafletLatLng).addTo(map);
    } catch (error) {
        console.error("Error creating mile marker:", error);
    }
}



document.addEventListener('DOMContentLoaded', function () {
    const startPointSelect = document.querySelector('#start-point-select')
    const endPointSelect = document.querySelector('#end-point-select')
    
    let startPoint = 5, endPoint = 7;

    startPointSelect.addEventListener('change', e => {
        const selectedOption = startPointSelect.querySelector('option:checked');
        console.log(selectedOption.dataset.mile)
        startPoint = selectedOption.dataset.mile
        
        if (endPoint) {
            drawPath(startPoint, endPoint);
        }
    })

    endPointSelect.addEventListener('change', e => {
        const selectedOption = endPointSelect.querySelector('option:checked');
        console.log(selectedOption.dataset.mile)
        endPoint = selectedOption.dataset.mile
        
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
            // const trailLayer = L.geoJSON(data, {
            //     style: trailStyle
            // }).addTo(map);

            // map.fitBounds(trailLayer.getBounds());
        })
        .catch(error => {
            console.error('Error loading the trail GeoJSON:', error);
        }).then(() => { drawPath(1, 7)});
});