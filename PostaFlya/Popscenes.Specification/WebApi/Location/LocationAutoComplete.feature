@locationfeatures
Feature: LocationAutoComplete
	In order to find a suburb
	I want to be able to search for one

Scenario: Search for a suburb or board
	Given There are 20 suburbs with 5 containing a word starting with the term bru
	When I perform a get request for the path webapi/search/byterms?q=bru
	Then I should receive a http response with a status of 200
	And The content should have a response status of OK
	Then the result list should contain 5 suburbs containing a word starting with one of:
	| Prefix |
	| bru    |
