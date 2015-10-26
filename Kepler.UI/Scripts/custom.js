﻿function loadModalWin(modalWinObj, modalWinAction) {
    switch (modalWinAction) {
        case 'create_proj':
            $('.modal-title').text('Create');
            $('.modal-body').empty().append('<div class="form-group"><label>Name</label><input class="form-control" name="proj_name" type="text"></div>');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-success">Save</button>');
            break;
        case 'edit_proj':
            $('.modal-title').text('Edit');
            $('.modal-body').empty().append('<div class="form-group"><label>Name</label><input class="form-control" type="text" name="proj_name" value="' + modalWinObj + '"></div>');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-success">Save</button>');
            break;
        case 'delete_proj':
            $('.modal-title').text('Delete');
            $('.modal-body').empty().append('Are you sure want to delete ' + modalWinObj + '?');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-danger">Yes</button>');
            break;
        case 'create_config':
            $('.modal-title').text('Create config');
            $('.modal-body').empty().append('<div class="fileupload fileupload-new" data-provides="fileupload"><span class="btn btn-file btn-default"><span class="fileupload-new">Select file</span><span class="fileupload-exists">Change</span><input type="file"></span><span class="fileupload-preview"></span><a href="#" class="close fileupload-exists" data-dismiss="fileupload" style="float: none">×</a></div><div class="alert alert-warning">Error AZAZA!!!</div>');
            $('.modal-footer').hide();
            break;
        case 'diff_path_edit':
            $('.modal-title').text('Edit diff image path');
            $('.modal-body').empty().append('<div class="form-group"><label>Path</label><input class="form-control" name="diff_path" type="text"></div>');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-success">Save</button>');
            break;
        case 'add_image_worker':
            $('.modal-title').text('Add image worker');
            $('.modal-body').empty().append('<div class="form-group"><label>Name</label><input class="form-control" name="image_worker_name" type="text"></div><div class="form-group"><label>Url</label><input class="form-control" name="image_worker_url" type="text"></div>');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-success">Save</button>');
            break;
        case 'edit_image_worker':
            $('.modal-title').text('Edit image worker');
            $('.modal-body').empty().append('<div class="form-group"><label>Name</label><input class="form-control" name="image_worker_name" type="text"></div><div class="form-group"><label>Url</label><input class="form-control" name="image_worker_url" type="text"></div>');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-success">Save</button>');
            break;
        case 'remove_image_worker':
            $('.modal-title').text('Remove image worker');
            $('.modal-body').empty().append('Are you sure want to delete it?');
            $('.modal-footer').show().empty().append('<button type="submit" class="btn btn-danger">Yes</button>');
            break;
    }
}

$("#modal_form").validate({
    rules: {
        proj_name: {
            required: true,
            minlength: 4,
            maxlength: 10,
        },
        diff_path: {
            required: true,
            minlength: 4,
            maxlength: 10,
        },
        image_worker_name: {
            required: true,
            minlength: 4,
            maxlength: 10,
        },
        image_worker_url: {
            required: true,
            url: true,
        },
    },
    messages: {
        proj_name: {
            required: "This field is required",
            minlength: "Minimum length of 4 symbol",
            maxlength: "Maximum length of 10 symbol",
        },
        diff_path: {
            required: "This field is required",
            minlength: "Minimum length of 4 symbol",
            maxlength: "Maximum length of 10 symbol",
        },
        image_worker_name: {
            required: "This field is required",
            minlength: "Minimum length of 4 symbol",
            maxlength: "Maximum length of 10 symbol",
        },
        image_worker_url: {
            required: "This field is required",
            url: "Please, enter valid URL: http://...",
        },
    }
});


/*====================================
METIS MENU 
======================================*/
(function ($) {
    "use strict";
    var mainApp = {

        metisMenu: function () {

            /*====================================
            METIS MENU 
            ======================================*/

            $('#main-menu').metisMenu();

        },


        loadMenu: function () {

            /*====================================
            LOAD APPROPRIATE MENU BAR
         ======================================*/

            $(window).bind("load resize", function () {
                if ($(this).width() < 768) {
                    $('div.sidebar-collapse').addClass('collapse')
                } else {
                    $('div.sidebar-collapse').removeClass('collapse')
                }
            });
        },
        slide_show: function () {

            /*====================================
           SLIDESHOW SCRIPTS
        ======================================*/

            $('#carousel-example').carousel({
                interval: 3000 // THIS TIME IS IN MILLI SECONDS
            })
        },
        reviews_fun: function () {
            /*====================================
         REWIEW SLIDE SCRIPTS
      ======================================*/
            $('#reviews').carousel({
                interval: 2000 //TIME IN MILLI SECONDS
            })
        },
        wizard_fun: function () {
            /*====================================
            //horizontal wizrd code section
             ======================================*/
            $(function () {
                //$("#wizard").steps({
                //    headerTag: "h2",
                //    bodyTag: "section",
                //    transitionEffect: "slideLeft"
                //});
            });
            /*====================================
            //vertical wizrd  code section
            ======================================*/
            $(function () {
                //$("#wizardV").steps({
                //    headerTag: "h2",
                //    bodyTag: "section",
                //    transitionEffect: "slideLeft",
                //    stepsOrientation: "vertical"
                //});
            });
        },
       
        
    };
    $(document).ready(function () {
        mainApp.metisMenu();
        mainApp.loadMenu();
        mainApp.slide_show();
        mainApp.reviews_fun();
        mainApp.wizard_fun();
       
    });
}(jQuery));