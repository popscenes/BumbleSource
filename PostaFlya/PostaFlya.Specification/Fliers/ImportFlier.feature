Feature: Flier Import
	In order to popukate the site
	As a user
	I want to be able to import fliers from other sources

@FlierImport
Scenario: Import Flier with Invalid Access Token
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I dont have a valid access token for the given source
	When I go to the flier import page for a source
	Then Then I will be redirected to obtain a valid token

Scenario: Import Flier with valid Access Token
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I have a valid access token for the given source
	When I go to the flier import page for a source
	Then Then potential fliers that have not already been imported with be displayed
