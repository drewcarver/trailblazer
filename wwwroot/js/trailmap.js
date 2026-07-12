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

// function getPointAlongMultiLine(multiLineGeoJSON, targetDistance, options = { units: 'miles' }) {
//   const flattened = turf.flatten(multiLineGeoJSON);
//   let rawSegments = [...flattened.features]; // Shallow copy to prevent mutating original data

//   if (rawSegments.length === 0) return null;

//   // 1. FIND THE TRUE STARTING POINT (Lowest Latitude / Southernmost Segment)
//   let lowestLat = Infinity;
//   let startIdx = 0;

//   rawSegments.forEach((seg, idx) => {
//     const coords = seg.geometry.coordinates;
//     const startLat = coords[0][1];
//     const endLat = coords[coords.length - 1][1];
//     const minLat = Math.min(startLat, endLat);
    
//     if (minLat < lowestLat) {
//       lowestLat = minLat;
//       startIdx = idx;
//     }
//   });

//   // Extract the true southern start segment (Fixing the splice array bug using [0])
//   let currentSegment = rawSegments.splice(startIdx, 1)[0];
  
//   // Ensure the starting segment points Northbound
//   if (currentSegment.geometry.coordinates[0][1] > currentSegment.geometry.coordinates[currentSegment.geometry.coordinates.length - 1][1]) {
//     currentSegment.geometry.coordinates.reverse();
//   }

//   const sortedSegments = [currentSegment];

//   // 2. CHAIN LINK GEOGRAPHIC SORTING
//   while (rawSegments.length > 0) {
//     const currentCoords = currentSegment.geometry.coordinates;
//     const endPoint = turf.point(currentCoords[currentCoords.length - 1]);

//     let closestIdx = 0;
//     let minDistance = Infinity;
//     let shouldFlip = false;

//     for (let i = 0; i < rawSegments.length; i++) {
//       const segCoords = rawSegments[i].geometry.coordinates;
//       const startOfNext = turf.point(segCoords[0]);
//       const endOfNext = turf.point(segCoords[segCoords.length - 1]);

//       const distToStart = turf.distance(endPoint, startOfNext, options);
//       const distToEnd = turf.distance(endPoint, endOfNext, options);

//       if (distToStart < minDistance) {
//         minDistance = distToStart;
//         closestIdx = i;
//         shouldFlip = false;
//       }
//       if (distToEnd < minDistance) {
//         minDistance = distToEnd;
//         closestIdx = i;
//         shouldFlip = true;
//       }
//     }

//     // CRITICAL FIX: Extract the actual object out of the spliced array
//     currentSegment = rawSegments.splice(closestIdx, 1)[0];

//     if (shouldFlip) {
//       currentSegment.geometry.coordinates.reverse();
//     }

//     sortedSegments.push(currentSegment);
//   }

//   // 3. CHRONOLOGICAL MILEAGE TRACKING
//   let accumulatedDistance = 0;

//   for (const segment of sortedSegments) {
//     const segmentLength = turf.length(segment, options);

//     if (accumulatedDistance + segmentLength >= targetDistance) {
//       const remainingDistanceNeeded = targetDistance - accumulatedDistance;
//       return turf.along(segment, remainingDistanceNeeded, options);
//     }

//     accumulatedDistance += segmentLength;
//   }

//   // Fallback: Total trail distance check / Final endpoint
//   console.log("Total Sorted Trail Length:", accumulatedDistance);
//   const lastSegment = sortedSegments[sortedSegments.length - 1];
//   const lastCoords = lastSegment.geometry.coordinates;
//   return turf.point(lastCoords[lastCoords.length - 1]);
// }

function getPointAlongMultiLine(trailJson, targetDistance, options = { units: 'miles' }) {
  const flattened = turf.flatten(trailJson);
  const segments = flattened.features;

  let accumulatedDistance = 0;

  // 2. Loop sequentially through each line segment
  for (const segment of segments) {
    const segmentLength = turf.length(segment, options);

    // 3. Check if our target distance lands inside this specific segment
    if (accumulatedDistance + segmentLength >= targetDistance) {
      const remainingDistanceNeeded = targetDistance - accumulatedDistance;
      
      // Call standard turf.along safely on just this matching single segment
      return turf.along(segment, remainingDistanceNeeded, options);
    }

    // Otherwise, add this segment's length and move to the next one
    accumulatedDistance += segmentLength;
  }

  // Fallback: If target distance overshoots the total trail, return the final endpoint
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