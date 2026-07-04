module HikePlanner.Views.ListPlans

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open Giraffe

let hikeRow (hike: Hike) = 
  tr [] [
    td [] [ str hike.Trail ]
  ]

let listPlans (hikes: Result<Hike list, HikeRepoError>)  = 
    (*body [ _class "bg-[#EDE4D5] text-gray-800" ] [
        nav [ _class "bg-[#2E5A3D] text-white sticky top-0 z-50 shadow-md" ] [
            div [ _class "max-w-7xl mx-auto px-6 py-4 flex items-center justify-between" ] [
                div [ _class "flex items-center gap-3" ] [
                    i [ _class "fa-solid fa-mountain text-3xl text-[#EDE4D5]" ] []
                    div [ _class "logo-font text-3xl font-bold tracking-tight" ] [ str "TrailForge" ]
                ]
                div [ _class "hidden md:flex items-center gap-8 text-sm font-medium" ] [
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Discover Trails" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Plan Hike" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "My Journal" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Past Hikes" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Community" ]
                ]
            ]
        ]
        match hikes with
          | Ok h -> table [] (h |> List.map hikeRow)
          | Error e -> div [] [ 
              match e with
                | DatabaseError e -> str e 
                | NotFound e -> str e 
          ] 
    ]*)
    rawText """
<div class="w-full overflow-x-auto border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200">
  <!-- Aesthetic Top Meta Label mirroring the 'TASK' / 'LIVE BROWSER' headers from Screenshot 2026-07-04 105924.png -->
  <div class="text-[10px] font-mono tracking-widest text-neutral-500 uppercase mb-2 pl-1">
    Hikes Registry // Active_Logs
  </div>

  <table class="w-full min-w-[600px] border-collapse text-left text-sm text-neutral-900 font-mono">
    <thead>
      <tr class="border border-black bg-neutral-100">
        <th scope="col" class="px-4 py-3 font-bold uppercase tracking-wider border-r border-black w-2/5">
          Hike Name
        </th>
        <th scope="col" class="px-4 py-3 font-bold uppercase tracking-wider border-r border-black w-1/5">
          Start Date
        </th>
        <th scope="col" class="px-4 py-3 font-bold uppercase tracking-wider border-r border-black w-1/5">
          End Date
        </th>
        <th scope="col" class="px-4 py-3 font-bold uppercase tracking-wider text-center w-1/5">
          Action
        </th>
      </tr>
    </thead>
    
    <tbody class="divide-y divide-black border-x border-b border-black">
      <!-- Row 1 -->
      <tr class="hover:bg-neutral-50 transition-colors">
        <td class="px-4 py-3 border-r border-black font-sans font-medium truncate max-w-[200px]">
          Pacific Crest Trail
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-07-10
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-07-24
        </td>
        <td class="px-4 py-3 text-center whitespace-nowrap">
          <button 
            type="button"
            aria-label="View details for Pacific Crest Trail"
            class="inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer"
          >
            View Hike
          </button>
        </td>
      </tr>

      <!-- Row 2 -->
      <tr class="hover:bg-neutral-50 transition-colors">
        <td class="px-4 py-3 border-r border-black font-sans font-medium truncate max-w-[200px]">
          Appalachian Trail
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-08-01
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-08-15
        </td>
        <td class="px-4 py-3 text-center whitespace-nowrap">
          <button 
            type="button"
            aria-label="View details for Appalachian Trail"
            class="inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer"
          >
            View Hike
          </button>
        </td>
      </tr>

      <!-- Row 3 -->
      <tr class="hover:bg-neutral-50 transition-colors">
        <td class="px-4 py-3 border-r border-black font-sans font-medium truncate max-w-[200px]">
          West Highland Way
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-09-05
        </td>
        <td class="px-4 py-3 border-r border-black whitespace-nowrap text-neutral-600">
          2026-09-12
        </td>
        <td class="px-4 py-3 text-center whitespace-nowrap">
          <button 
            type="button"
            aria-label="View details for West Highland Way"
            class="inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer"
          >
            View Hike
          </button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
    """
    |> withMasterLayout
