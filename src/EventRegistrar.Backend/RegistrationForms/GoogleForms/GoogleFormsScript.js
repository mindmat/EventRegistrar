/**
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * @OnlyCurrentDoc
 *
 * The above comment directs Apps Script to limit the scope of file
 * access for this add-on. It specifies that this add-on will only
 * attempt to read or modify the files in which the add-on is used,
 * and not all of the user's files. The authorization request message
 * presented to users will reflect this limited scope.
 */
var ADDON_TITLE = 'Event Registrar';
var baseUrl = 'https://event-admin-functions.azurewebsites.net';
var eventAcronym = '{eventAcronym}';

function onSubmit(e) {
    postAnswerToBackend(e.response);
}

function postAnswerToBackend(response) {
    var responsesData = [];
    var itemResponses = response.getItemResponses();
    for (var i = 0; i < itemResponses.length; i++) {
        var itemResponse = itemResponses[i];
        var stringAnswer = '';
        var stringArrayAnswer = [];
        var type = itemResponse.getItem().getType();
        if (type == FormApp.ItemType.CHECKBOX || type == FormApp.ItemType.GRID) {
            stringArrayAnswer = itemResponse.getResponse()
        }
        else {
            stringAnswer = itemResponse.getResponse();
        }
        var responseItem =
        {
            questionExternalId: itemResponse.getItem().getId(),
            response: stringAnswer,
            responses: stringArrayAnswer
        };
        responsesData.push(responseItem);
    }

    var responseData =
    {
        email: response.getRespondentEmail(), // collect email via form option
        timestamp: response.getTimestamp(),
        responses: responsesData
    };

    var options = {
        'method': 'post',
        'contentType': 'application/json',
        'payload': JSON.stringify(responseData)
    };

    var form = FormApp.getActiveForm();
    var url = baseUrl + '/api/events/' + eventAcronym + '/registrationforms/' + form.getId() + '/registrations/' + response.getId();
    UrlFetchApp.fetch(url, options);
}

/**
 * Runs when the add-on is installed.
 *
 * @param {object} e The event parameter for a simple onInstall trigger. To
 *     determine which authorization mode (ScriptApp.AuthMode) the trigger is
 *     running in, inspect e.authMode. (In practice, onInstall triggers always
 *     run in AuthMode.FULL, but onOpen triggers may be AuthMode.LIMITED or
 *     AuthMode.NONE).
 */
function onInstall(e) {
    onOpen(e);
}

/**
 * Adds a custom menu to the active form to show the add-on sidebar.
 *
 * @param {object} e The event parameter for a simple onOpen trigger. To
 *     determine which authorization mode (ScriptApp.AuthMode) the trigger is
 *     running in, inspect e.authMode.
 */
function onOpen(e) {
    FormApp.getUi()
        .createAddonMenu()
        .addItem('Update Questions in Event Registrator', 'updateFormDefinitionInEventRegistrator')
        //.addItem('Resync answers', 'resyncAnswers')
        .addItem('Resync missing answers', 'resyncMissingAnswers')
        .addToUi();
}

function updateFormDefinitionInEventRegistrator() {
    var form = FormApp.getActiveForm();

    var questions = [];
    var formItems = form.getItems();
    for (var i = 0; i < formItems.length; i++) {
        var formItem = formItems[i];
        var question =
        {
            id: formItem.getId(),
            type: formItem.getType().toString(),
            title: formItem.getTitle(),
            index: formItem.getIndex()
        }
        if (formItem.getType() == FormApp.ItemType.MULTIPLE_CHOICE) {
            var multipleChoiceItem = formItem.asMultipleChoiceItem();
            var choices = multipleChoiceItem.getChoices();
            question.choices = [];
            for (var j = 0; j < choices.length; j++) {
                question.choices.push(choices[j].getValue());
            }
        }
        else if (formItem.getType() == FormApp.ItemType.LIST) {
            var listItem = formItem.asListItem();
            var choices = listItem.getChoices();
            question.choices = [];
            for (var j = 0; j < choices.length; j++) {
                question.choices.push(choices[j].getValue());
            }
        }
        else if (formItem.getType() == FormApp.ItemType.CHECKBOX) {
            var checkboxGridItem = formItem.asCheckboxItem();
            var choices = checkboxGridItem.getChoices();
            question.choices = [];
            for (var j = 0; j < choices.length; j++) {
                question.choices.push(choices[j].getValue());
            }
        }
        questions.push(question);
    }

    var registrationForm =
    {
        title: form.getTitle(),
        'questions': questions
    }

    var options = {
        'method': 'post',
        'contentType': 'application/json',
        'payload': JSON.stringify(registrationForm)
    };
    var url = baseUrl + '/api/events/' + eventAcronym + '/registrationforms/' + form.getId();
    var httpResult = UrlFetchApp.fetch(url, options);

    if (httpResult.getResponseCode() != 200) {
        FormApp.getUi().alert('request result code ' + httpResult.getResponseCode());
        FormApp.getUi().alert(httpResult.getContentText());
        return;
    }
}

/*
function resyncAnswers() {
  var form = FormApp.getActiveForm();
  var responses = form.getResponses();
  for (var i = 0; i < responses.length; i++) {
    try {
      postAnswerToBackend(responses[i]);
    }
    catch (err) {
    }
  }
}
*/

function resyncMissingAnswers() {
    var form = FormApp.getActiveForm();
    var options = {
        'method': 'get'
    };
    var url = baseUrl + '/api/registrationforms/' + form.getId() + '/RegistrationExternalIdentifiers';
    var httpResult = UrlFetchApp.fetch(url, options);

    if (httpResult.getResponseCode() != 200) {
        FormApp.getUi().alert('request result code ' + httpResult.getResponseCode());
        return;
    }

    submittedResponseIds = JSON.parse(httpResult.getContentText());

    var responses = form.getResponses();
    var submitsLeft = 5;
    //  for (var i = 0; i < responses.length; i++) {
    for (var i = 0; i < responses.length; i++) {
        var response = responses[i];
        var responseId = response.getId();
        var matchFound = false;
        for (j in submittedResponseIds) {
            if (submittedResponseIds[j] == responseId) {
                matchFound = true;
                break;
            }
        }
        if (matchFound) {
            // response is transmitted, no work to do
            continue;
        }

        try {
            postAnswerToBackend(response);
            submitsLeft--;
            //FormApp.getUi().alert('missing response ' + responseId);
        }
        catch (err) {
        }
        if (submitsLeft <= 0) {
            break;
        }
    }
}