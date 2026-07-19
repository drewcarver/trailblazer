namespace HikePlanner.Tests

open System
open Xunit
open HikePlanner.Handlers.Common
open HikePlanner.Core

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
