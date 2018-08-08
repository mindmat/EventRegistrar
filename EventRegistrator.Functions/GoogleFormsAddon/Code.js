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

/**
 * A global constant String holding the title of the add-on. This is
 * used to identify the add-on in the notification emails.
 */
var ADDON_TITLE = 'Event Registrator';
var eventAcronym = 'rb18';

/**
 * A global constant 'notice' text to include with each email
 * notification.
 */
var NOTICE = "Form Notifications was created as an sample add-on, and is meant for \
demonstration purposes only. It should not be used for complex or important \
workflows. The number of notifications this add-on produces are limited by the \
owner's available email quota; it will not send email notifications if the \
owner's daily email quota has been exceeded. Collaborators using this add-on on \
the same form will be able to adjust the notification settings, but will not be \
able to disable the notification triggers set by other collaborators.";

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
      //email: response.getRespondentEmail(),
      timestamp: response.getTimestamp(),
      responses: responsesData
    };

  var options = {
    'method': 'post',
    'contentType': 'application/json',
    'payload': JSON.stringify(responseData)
  };

  var form = FormApp.getActiveForm();
  var url = 'https://eventregistrarfunctions.azurewebsites.net/api/events/' + eventAcronym + '/registrationforms/' + form.getId() + '/registrations/' + response.getId();
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

  var registrationFrom =
    {
      title: form.getTitle(),
      'questions': questions
    }

  var options = {
    'method': 'post',
    'contentType': 'application/json',
    'payload': JSON.stringify(registrationFrom)
  };
  var url = 'https://eventregistratorweb.azurewebsites.net/api/events/' + eventAcronym + '/registrationforms/' + form.getId();
  UrlFetchApp.fetch(url, options);
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
  var url = 'https://eventregistratorweb.azurewebsites.net/api/registrationform/' + form.getId() + '/ExternalIdentifiers';
  var httpResult = UrlFetchApp.fetch(url, options);

  if (httpResult.getResponseCode() != 200) {
    FormApp.getUi().alert('request result code ' + httpResult.getResponseCode());
    return;
  }

  submittedResponseIds = JSON.parse(httpResult.getContentText());

  var responses = form.getResponses();

  //  for (var i = 0; i < responses.length; i++) {
  for (var i = 0; i < 5; i++) {
    var response = responses[i];
    var responseId = response.getId();
    var matchFound = false;
    for (i in submittedResponseIds) {
      if (submittedResponseIds[i] == responseId) {
        matchFound = true;
        break;
      }
    }
    if (matchFound) {
      // response is transmitted, no work to do
      continue;
    }

    try {
      //postAnswerToBackend(response);
      FormApp.getUi().alert('missing response ' + responseId);
    }
    catch (err) {
    }
  }
}