const trailStyle = {
    color: "#ff3300",  // Bright red-orange line
    weight: 4,         // Thickness of the line
    opacity: 0.85,     // Transparency
    lineJoin: 'round'  // Smooth intersections
};

let activeMileMarker;
let hikeLayers = new Map();

function removeHikeLayer(day) {
    if (hikeLayers.has(day)) {
        window.$map.removeLayer(hikeLayers.get(day));
        hikeLayers.delete(day);
    }
}

function drawPath(startMile, endMile, day) {
    if (!window.$trailGeoJSONData) {
        console.error("Trail data hasn't loaded yet!");
        return;
    }
    
    if (hikeLayers.has(day)) {
        window.$map.removeLayer(hikeLayers.get(day));
    }

    const pathColors = ["#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd", "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf"];
    const pathColor = pathColors[(day - 1) % pathColors.length];

    try {
        const flattened = turf.flatten(window.$trailGeoJSONData);
        let pathLine;

        if (flattened.features.length === 1) {
            pathLine = turf.lineSliceAlong(flattened.features[0], startMile, endMile, { units: 'miles' });
        } else {
            let accumulatedDistance = 0;
            let pathCoordinates = [];

            for (const segment of flattened.features) {
                const segmentLength = turf.length(segment, { units: 'miles' });
                const nextDistance = accumulatedDistance + segmentLength;

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

        const pathLayer = L.geoJSON(pathLine, {
            style: {
                color: pathColor,
                weight: 6,         
                opacity: 0.9,
                lineJoin: 'round'
            }
        }).addTo(window.$map);

        pathLayer.bindTooltip(`Day ${day - 1} to ${day}: ${endMile - startMile} miles`, {
            permanent: true,
            direction: 'center',
            className: 'trail-tooltip'
        });

        hikeLayers.set(day, pathLayer);

        const combinedBounds = L.latLngBounds();
        for (const layer of hikeLayers.values()) {
            combinedBounds.extend(layer.getBounds());
        }
        window.$map.fitBounds(combinedBounds);
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
    if (!window.$trailGeoJSONData) {
        console.error("Trail data hasn't loaded yet!");
        return;
    }

    if (activeMileMarker) {
        window.$map.removeLayer(activeMileMarker);
    }

    try {
        const targetPoint = getPointAlongMultiLine(window.$trailGeoJSONData, miles);
        const coords = targetPoint.geometry.coordinates;
        
        const leafletLatLng = [coords[1], coords[0]];

        activeMileMarker = L.marker(leafletLatLng).addTo(window.$map);
    } catch (error) {
        console.error("Error creating mile marker:", error);
    }
}
