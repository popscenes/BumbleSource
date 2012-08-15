Feature: Flier Import
	In order to popukate the site
	As a user
	I want to be able to import fliers from other sources

@FlierImport
Scenario: Import Flier with Invalid Access Token
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I dont have a valid access token for the given source
	When I go to the flier import page for a source
	Then Then i will be prompted to obtain a valid token

Scenario: Import Flier with valid Access Token
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I have a valid access token for the given source
	When I go to the flier import page for a source
	Then Then potential fliers that have not already been imported with be displayed

Scenario: Import Flier Obtain valid Access Token
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I do not have a valid acces token to import fliers
	When I obtain a valid token from the source
	Then Then i will be redircted back to the flier import page
	And I will have a valid access token for the source
