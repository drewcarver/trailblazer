namespace HikePlanner.Tests

open System
open System.Net
open Xunit
open HikePlanner.Handlers.Handlers

module SaveHikePlanRouteTests =
    [<Fact>]
    let ``POST plan saves hike and returns HTMX headers`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = [ "friend1@example.com"; "friend2@example.com" ]
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! response = testContext.Client.PostAsync("/plan", form)

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
            Assert.Equal("/plan", response.Headers.GetValues("HX-Location") |> Seq.head)
            Assert.Equal("1", response.Headers.GetValues("x-hike-id") |> Seq.head)
        }

    [<Fact>]
    let ``POST plan with blank hike name returns validation message`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let invalidForm: SaveHikeForm = {
                HikeName = "   "
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent invalidForm
            let! response = testContext.Client.PostAsync("/plan", form)

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.False(response.Headers.Contains("x-hike-id"))
        }
