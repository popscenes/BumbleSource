@bulletinfeatures
Feature: FlyersByFeatured
	In order to find gigs
	As a browser
	I want to be able to search for gigs by most recent posts

Scenario: Mobile Api flyers by featured returns flyers
	Given There are 50 flyers within 100 kilometers of the geolocation -37.769, 144.979  with dates starting from 2013-06-29
	When I perform a get request for the path mobileapi/gigs?take=30 
	Then I should receive a http response with a status of 200
	And The content should have a response status of OK
	And The content should contain a list of 30 flyers ordered by created date desc

#Scenario: Mobile Api flyers by featured pages to next flyers
#	Given There are 50 flyers within 10 kilometers of the geolocation -37.769, 144.979
#	And I have retrieved the latest 30 flyers using mobileapi/gigs?take={0}
#	When I attempt to retrieve the next 30 latest flyers using mobileapi/gigs?take={0}&skip={1}
#	Then I should receive a http response with a status of 200
#	And The content should have a response status of OK
#	And The content should contain a list of 20 flyers ordered by created date desc


