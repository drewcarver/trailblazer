namespace Trailblazer.UnitTests

open System
open System.Net
open Xunit
open HikePlanner.Handlers.Handlers

module SaveHikeRouteTests =
    [<Fact>]
    let ``POST hikes saves hike and returns HTMX headers`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = [ "friend1@example.com"; "friend2@example.com" ]
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! response = testContext.Client.PostAsync("/hikes", form)

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
            Assert.Equal("/hikes", response.Headers.GetValues("HX-Location") |> Seq.head)
            Assert.Equal("1", response.Headers.GetValues("x-hike-id") |> Seq.head)
        }

    [<Fact>]
    let ``POST hikes with blank hike name returns validation message`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let invalidForm: SaveHikeForm = {
                HikeName = "   "
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent invalidForm
            let! response = testContext.Client.PostAsync("/hikes", form)

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
            Assert.False(response.Headers.Contains("x-hike-id"))
        }

    [<Fact>]
    let ``POST hikes by id updates hike and updated hike appears in hikes table`` () =
        task {
            use! testContext = TestSupport.buildTestContext()

            let originalForm: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = [ "friend1@example.com"; "friend2@example.com" ]
            }

            use createForm = TestSupport.buildSaveHikeFormContent originalForm
            let! createResponse = testContext.Client.PostAsync("/hikes", createForm)
            let createdHikeId = createResponse.Headers.GetValues("x-hike-id") |> Seq.head

            Assert.Equal(HttpStatusCode.NoContent, createResponse.StatusCode)
            Assert.False(String.IsNullOrWhiteSpace createdHikeId)

            let updatedForm: SaveHikeForm = {
                HikeName = "Foggy Ridge"
                StartDate = DateTime(2026, 7, 20)
                CampPoints = [ "1"; "3" ]
                Invitees = [ "friend1@example.com" ]
            }

            use editForm = TestSupport.buildSaveHikeFormContent updatedForm
            let! editResponse = testContext.Client.PostAsync(sprintf "/hikes/%s" createdHikeId, editForm)

            Assert.True(editResponse.StatusCode = HttpStatusCode.NoContent)
            let redirectLocation = editResponse.Headers.GetValues("HX-Location") |> Seq.head
            Assert.Equal("/hikes", redirectLocation)
            Assert.Equal(createdHikeId, editResponse.Headers.GetValues("x-hike-id") |> Seq.head)

            let! listResponse = testContext.Client.GetAsync(redirectLocation)
            let! listContent = listResponse.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode)
            Assert.Contains("My Hikes", listContent)
            Assert.Contains("Foggy Ridge", listContent)
        }

