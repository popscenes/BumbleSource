@bulletinfeatures
Feature: FlyersByLatest
	In order to find gigs
	As a browser
	I want to be able to search for gigs by most recent posts

Scenario: Mobile Api flyers by latest returns latest flyers
	Given There are 50 flyers within 100 kilometers of the geolocation -37.769, 144.979
	When I perform a get request for the path mobileapi/gigs/latest?take=30 
	Then I should receive a http response with a status of 200
	And The content should have a response status of OK
	And The content should contain a list of 30 flyers ordered by created date desc
