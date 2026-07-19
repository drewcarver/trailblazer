namespace HikePlanner.Tests

open System
open System.Net
open Xunit
open HikePlanner.Handlers.Handlers

module PlanRoutesTests =
    [<Fact>]
    let ``GET plan create returns page with create title`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let! response = testContext.Client.GetAsync("/plan/create")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Create New Hike", content)
        }

    [<Fact>]
    let ``GET plan lists saved hikes`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! _ = testContext.Client.PostAsync("/plan", form)

            let! response = testContext.Client.GetAsync("/plan")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("My Hikes", content)
        }

    [<Fact>]
    let ``GET plan by unknown id shows error view`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let! response = testContext.Client.GetAsync("/plan/999")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("An error occurred while retrieving the hike details.", content)
            Assert.Contains("Back to Plans", content)
        }
