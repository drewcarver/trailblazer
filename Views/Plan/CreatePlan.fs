module HikePlanner.Views.Plan.Plan

open System;
open Giraffe.ViewEngine
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.Components.Select
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core

let planView (trailPointsOfInterest: Result<TrailPointOfInterest list, TrailblazerError>) =
        let toOptionLabel poi = sprintf "%s - Mile %.2f" poi.Name poi.TrailMile
        let pointsOfInterestOptions = 
            trailPointsOfInterest 
            |> Result.defaultWith (fun _ -> List.empty)
            |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = string poi.Id; Attributes = [ ("data-mile", poi.TrailMile.ToString())] })

        div [] [
            script [] [
                rawText """
                const trailStyle = {
                    color: "#ff3300",  // Bright red-orange line
                    weight: 4,         // Thickness of the line
                    opacity: 0.85,     // Transparency
                    lineJoin: 'round'  // Smooth intersections
                };

                document.addEventListener('DOMContentLoaded', function () {
                    var map = L.map('map').setView([34.628, -84.193], 13); // Springer Mountain, Georgia

                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    }).addTo(map);

                    fetch('/trails/AppalachianTrail.json')
                        .then(response => {
                            if (!response.ok) {
                                throw new Error('Network response was not ok');
                            }
                            return response.json();
                        })
                        .then(data => {
                            console.log(data)
                            const trailLayer = L.geoJSON(data, {
                                style: trailStyle
                            }).addTo(map);

                            map.fitBounds(trailLayer.getBounds());
                        })
                        .catch(error => {
                            console.error('Error loading the trail GeoJSON:', error);
                        });
                });
                """
            ]
            div [ _id "map"; _class "w-[90vw] h-[400px]" ] []
            form [ _class "max-w-3xl mx-auto mt-8 p-6 bg-white rounded-3xl shadow-md border border-[#D4C3A8]"; attr "hx-post" "/plan" ] [
                textInput "hike-name" "hikeName" "Hike Name" true
                datePicker "start-date" "startDate" "Start Date" (Some "
                    on change or load
                        set #end-date.min to my value
                ") (Some DateTime.Now)
                datePicker "end-date" "endDate" "End Date" (Some "
                    on change or load
                        set #start-date.max to my value
                ") (Some DateTime.Now)
                trailblazerSelect "start-point-select" "startPointId" "Starting Point" pointsOfInterestOptions (Some "
                    on change 
                        set startMile to parseFloat(my selectedOptions.dataset.mile)
          
                        for opt in #end-point-select.options
                            set endMile to parseFloat(opt.dataset.mile)

                            if endMile < startMile
                                set opt.disabled to true
                            else
                                set opt.disabled to false
                            end
                        end
          
                        if #end-point-select.selectedOptions[0].disabled
                            set #end-point-select.value to ''
                ") []
                match trailPointsOfInterest with
                    | Ok _ -> emptyText
                    | Error e -> span [] [ 
                        match e with
                            | DatabaseError error -> str "An error ocurred when retrieving points of interest." 
                            | _ -> emptyText
                     ]
                trailblazerSelect "end-point-select" "endPointId" "Ending Point" pointsOfInterestOptions (Some "
                    on change 
                        set endMile to parseFloat(my selectedOptions.dataset.mile)
          
                        for opt in #start-point-select.options
                            set startMile to parseFloat(opt.dataset.mile)

                            if endMile < startMile
                                set opt.disabled to true
                            else
                                set opt.disabled to false
                            end
                        end
          
                        if #start-point-select.selectedOptions[0].disabled
                            set #start-point-select.value to ''
                ") []
                match trailPointsOfInterest with
                    | Ok _ -> emptyText
                    | Error e -> span [] [ 
                        match e with
                            | FormValidationError error -> str error
                            | _ -> str "An error has occurred"
                     ]
                button [ _type "submit"; _class "bg-[#4A7043] hover:bg-[#2E5A3D] text-white px-6 py-2 rounded-full font-medium transition-colors" ] [ str "Submit Plan" ]
            ]
        ] |> withMasterLayout
