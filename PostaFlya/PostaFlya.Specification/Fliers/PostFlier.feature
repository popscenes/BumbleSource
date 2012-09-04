﻿Feature: PARTICIPANT can POST FLIER
	As a BROWSER in a PARTICIPANT role
	I want to be able to POST a FLIER 
	so that it can be included in a DYNAMIC BULLETIN BOARD

Scenario: Create Flier With Default Behaviour
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be ACTIVE

Scenario: Edit Flier
Given I have created a FLIER 
When I navigate to the edit page for that FLIER and update any of the required data for a FLIER 
Then the FLIER will be updated to reflect those changes

Scenario: Attach Images to an existing flier
Given I have created a FLIER 
When I add images to the FLIER
Then The FLIER will contain the extra images

Scenario: Create Flier With Contact Details
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to attach my default contact details
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be ACTIVE
And contact details will be retrievable for the FLIER