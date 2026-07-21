namespace HikePlanner.Handlers

module Handlers =
    type SaveHikeForm = Common.SaveHikeForm

    let getUserProfile<'env> = Common.getUserProfile<'env>
    let accountHandler = AccountHandler.accountHandler
    let listPlansHandler = ListPlansHandler.listPlansHandler
    let planHandler = CreatePlanHandler.planHandler
    let saveHikePlan = SaveHikePlanHandler.saveHikePlan
    let updateHikePlan = SaveHikePlanHandler.updateHikePlan
    let viewHikeHandler = ViewHikeHandler.viewHikeHandler
