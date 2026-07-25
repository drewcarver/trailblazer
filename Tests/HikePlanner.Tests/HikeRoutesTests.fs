namespace HikePlanner.Tests

open System
open System.Net
open System.Security.Claims
open Xunit
open HikePlanner.Handlers.Handlers

module HikeRoutesTests =
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
            let! saveResponse = testContext.Client.PostAsync("/hikes", form)
            let savedId = saveResponse.Headers.GetValues("x-hike-id") |> Seq.head

            let! response = testContext.Client.GetAsync(sprintf "/hikes/%s" savedId)
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

    [<Fact>]
    let ``GET hikes only returns hikes for the signed in user`` () =
        task {
            let sharedConnectionString =
                sprintf "Data Source=file:%s?mode=memory&cache=shared" (Guid.NewGuid().ToString("N"))

            let ownerUser =
                ClaimsPrincipal(
                    ClaimsIdentity(
                        [
                            Claim(ClaimTypes.Name, "Owner User")
                            Claim(ClaimTypes.Email, "owner@example.com")
                        ],
                        "Test"
                    )
                )

            let otherUser =
                ClaimsPrincipal(
                    ClaimsIdentity(
                        [
                            Claim(ClaimTypes.Name, "Other User")
                            Claim(ClaimTypes.Email, "other@example.com")
                        ],
                        "Test"
                    )
                )

            use! ownerContext = TestSupport.buildTestContextWithConnectionString sharedConnectionString (Some ownerUser)
            use! otherContext = TestSupport.buildTestContextWithConnectionString sharedConnectionString (Some otherUser)

            let formToSave: SaveHikeForm = {
                HikeName = "Mossy Peak"
                StartDate = DateTime(2026, 7, 19)
                CampPoints = [ "1"; "2" ]
                Invitees = []
            }

            use form = TestSupport.buildSaveHikeFormContent formToSave
            let! saveResponse = ownerContext.Client.PostAsync("/hikes", form)
            let savedId = saveResponse.Headers.GetValues("x-hike-id") |> Seq.head

            let! listResponse = otherContext.Client.GetAsync("/hikes")
            let! listContent = listResponse.Content.ReadAsStringAsync()
            let! detailResponse = otherContext.Client.GetAsync(sprintf "/hikes/%s" savedId)
            let! detailContent = detailResponse.Content.ReadAsStringAsync()

            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode)
            Assert.DoesNotContain("Mossy Peak", listContent)
            Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode)
            Assert.DoesNotContain("value=\"Mossy Peak\"", detailContent)
        }
