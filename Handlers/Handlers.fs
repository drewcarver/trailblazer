namespace HikePlanner.Handlers

module Handlers =
    type SaveHikeForm = Common.SaveHikeForm

    let getUserProfile<'env> = Common.getUserProfile<'env>
    let accountHandler = AccountHandler.accountHandler
    let listHikesHandler = ListHikesHandler.listHikesHandler
    let createHikeHandler = CreateHikeHandler.createHikeHandler
    let saveHikeHandler = SaveHikeHandler.saveHikeHandler
    let updateHikeHandler = SaveHikeHandler.updateHikeHandler
    let viewHikeHandler = ViewHikeHandler.viewHikeHandler
