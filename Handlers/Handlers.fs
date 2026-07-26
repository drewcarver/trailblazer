namespace HikePlanner.Handlers

module Handlers =
    type SaveHikeForm = Common.SaveHikeForm

    let homeHandler = HomeHandler.homeHandler
    let accountHandler = AccountHandler.accountHandler
    let listHikesHandler = ListHikesHandler.listHikesHandler
    let listHikesViewHandler = ListHikesViewHandler.listHikesViewHandler
    let createHikeHandler = CreateHikeHandler.createHikeHandler
    let saveHikeHandler = SaveHikeHandler.saveHikeHandler
    let updateHikeHandler = SaveHikeHandler.updateHikeHandler
    let viewHikeHandler = ViewHikeHandler.viewHikeHandler
