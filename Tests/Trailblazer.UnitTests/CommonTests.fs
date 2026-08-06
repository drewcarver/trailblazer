namespace Trailblazer.UnitTests

open System
open System.Security.Claims
open Microsoft.AspNetCore.Http
open Nelknet.LibSQL.Data
open Xunit
open HikePlanner.Handlers.Common
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers

module CommonTests =
    [<Fact>]
    let ``validateSaveHikeForm trims name and removes blank invitees`` () =
        let form = {
            HikeName = "  Mossy Peak  "
            StartDate = DateTime(2026, 7, 19)
            CampPoints = [ "1"; "2" ]
            Invitees = [ "friend1@example.com"; "  "; "friend2@example.com" ]
        }

        let result = validateSaveHikeForm form

        match result with
        | Ok normalized ->
            Assert.Equal("Mossy Peak", normalized.HikeName)
            Assert.Equal<string list>([ "friend1@example.com"; "friend2@example.com" ], normalized.Invitees)
        | Error _ -> Assert.Fail("Expected form to be valid.")

    [<Fact>]
    let ``validateSaveHikeForm rejects blank hike name`` () =
        let form = {
            HikeName = "   "
            StartDate = DateTime(2026, 7, 19)
            CampPoints = [ "1" ]
            Invitees = []
        }

        let result = validateSaveHikeForm form

        match result with
        | Ok _ -> Assert.Fail("Expected form validation failure.")
        | Error (FormValidationError message) -> Assert.Equal("HikeName is required.", message)
        | Error _ -> Assert.Fail("Expected FormValidationError.")

    [<Fact>]
    let ``validateSaveHikeForm rejects empty camp points`` () =
        let form = {
            HikeName = "Mossy Peak"
            StartDate = DateTime(2026, 7, 19)
            CampPoints = []
            Invitees = []
        }

        let result = validateSaveHikeForm form

        match result with
        | Ok _ -> Assert.Fail("Expected form validation failure.")
        | Error (FormValidationError message) -> Assert.Equal("At least one camp point is required.", message)
        | Error _ -> Assert.Fail("Expected FormValidationError.")

    [<Fact>]
    let ``getUserProfile prefers stored avatar over Google claim URL`` () =
        task {
            let dbName = sprintf "trailblazer-common-tests-%s" (Guid.NewGuid().ToString("N"))
            let connectionString = sprintf "Data Source=file:%s?mode=memory&cache=shared" dbName

            use keepAliveConnection = new LibSQLConnection(connectionString)
            do! keepAliveConnection.OpenAsync()

            let storedAvatar = "data:image/png;base64,c3RvcmVkLWF2YXRhcg=="
            let httpContext = DefaultHttpContext()
            httpContext.User <-
                ClaimsPrincipal(
                    ClaimsIdentity(
                        [
                            Claim(ClaimTypes.Name, "Test User")
                            Claim(ClaimTypes.Email, "test@example.com")
                            Claim("urn:google:picture", "https://example.com/google-avatar.png")
                        ],
                        "Test"
                    )
                )

            let env = {
                Environment = { ConnectionString = ConnectionString connectionString }
                Context = httpContext
            }

            let! saveResult =
                saveUser {
                    Email = "test@example.com"
                    Name = "Stored User"
                    Picture = Some storedAvatar
                    Friends = []
                }
                |> App.run env

            match saveResult with
            | Error error -> Assert.Fail(sprintf "Expected user save to succeed, got %A" error)
            | Ok () -> ()

            let! result = getUserProfile |> App.run env

            match result with
            | Ok profile -> Assert.Equal(Some storedAvatar, profile.Picture)
            | Error error -> Assert.Fail(sprintf "Expected user profile lookup to succeed, got %A" error)
        }
