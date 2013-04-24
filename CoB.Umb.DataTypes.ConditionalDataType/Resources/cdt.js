function cdtInit(cdt) {
    
    if (!cdt.showLabel) {
        $(cdt.id).parent().siblings('.propertyItemheader').hide();
    }

    if (cdt.triggerId) {

        var $elem = $(cdt.id).parentsUntil('.propertypane').last().parent().parent();
        var query, event, condition;
        var set = false;

        switch (cdt.type)
        {
            case 'CheckBox':
                query = cdt.triggerId;
                event = 'click';
                condition = function (elem, value) { return $(elem).is(':checked').toString() == value; };
                set = true;
                break;

            case 'CheckBoxList':
                query = cdt.triggerId + ' :checkbox';
                event = 'click';
                condition = function (elem, value, query) {
                    var retval = false;
                    $(query).each(function () {
                        if ($(this).is(':checked') && $(this).val() == value) {
                            retval = true;
                            return false;
                        }
                    });
                    return retval;
                };
                set = true;
                break;

            case 'RadioButtonList':
                query = cdt.triggerId + ' :radio';
                event = 'click';
                condition = function (elem, value) { return $(elem).val() == value; };
                set = true;
                break;

            case 'DropDownList':
                query = cdt.triggerId;
                event = 'change';
                condition = function (elem, value) { return $(elem).val() == value; };
                set = true;
                break;
        }

        if (set)
            initAndBind($elem, query, event, condition);
    }

    function initAndBind($elem, query, event, condition) {
        /// <summary>
        /// Sets the display state of the element on load and then binds
        /// the show/hide condition to the trigger element.
        /// </summary>

        // Hide by default.
        $elem.hide(); 

        var values = cdt.triggerValue.split(',');
        var show = true;
        
        // Un-hide if condition is met.
        $(query).each(function () {
            for (i in values) {
                if (condition(this, values[i], query)) {
                    $elem.show();
                    break;
                }
            }
        });

        // Bind condition check to event.
        $(query).bind(event, function () {

            var show = false;
            for (i in values) {
                if (condition(this, values[i], query)) {
                    show = true;
                    break;
                }
            }
            if (show)
                $elem.animate({ opacity: 'show', height: 'show' });
            else
                $elem.animate({ opacity: 'hide', height: 'hide' });
        });
    }
}