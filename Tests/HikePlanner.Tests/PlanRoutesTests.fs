namespace HikePlanner.Tests

open System
open System.Net
open Xunit
open HikePlanner.Handlers.Handlers

module PlanRoutesTests =
    [<Fact>]
    let ``GET hikes create returns page with create title`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let! response = testContext.Client.GetAsync("/hikes/create")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Create New Hike", content)
        }

    [<Fact>]
    let ``GET hikes lists saved hikes`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! _ = testContext.Client.PostAsync("/hikes", form)

            let! response = testContext.Client.GetAsync("/hikes")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("My Hikes", content)
        }

    [<Fact>]
    let ``GET hikes by id shows edit page`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! _ = testContext.Client.PostAsync("/hikes", form)

            let! response = testContext.Client.GetAsync("/hikes/1")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Edit Hike", content)
            Assert.Contains("value=\"Mossy Peak\"", content)
        }

    [<Fact>]
    let ``GET hikes by unknown id shows edit page with error`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let! response = testContext.Client.GetAsync("/hikes/999")
            let! content = response.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.Contains("Edit Hike", content)
        }
