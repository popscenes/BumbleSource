@bulletinfeatures
Feature: FlyersByLocationFeature
	In order to find gigs
	As a browser
	I want to be able to search for gigs around my area



Scenario: Mobile Api flyers by location with valid location returns all fliers within specified distance
	Given There are 50 flyers within 10 kilometers of the geolocation -37.769, 144.979 with dates starting from 2013-06-29T00:00:00+10:00
	When I perform a get request for the path mobileapi/gigs/bydate?lat=-37.769&lng=144.979&distance=10&start=2013-06-29&end=2013-07-02
	Then I should receive a http response with a status of 200
	And The content should have a response status of OK
	And The content should contain a list of flyers within 10 kilometers of -37.769, 144.979 in the date range 2013-06-29T00:00:00+10:00 to 2013-07-02T00:00:00+10:00

#Scenario: Mobile Api flyers by location pages to next flyers
#	Given There are 50 flyers within 10 kilometers of the geolocation -37.769, 144.979
#	And I have retrieved the first 30 flyers within 10 kilometers of -37.769, 144.979 using mobileapi/gigs/bydate?lat={0}&long={1}&distance={2}&take={3}
#	When I attempt to retrieve the next 30 flyers within 10 kilometers of -37.769, 144.979 using mobileapi/gigs/near?lat={0}&long={1}&distance={2}&take={3}&skip={4}
#	Then I should receive a http response with a status of 200
#	And The content should have a response status of OK
#	And The content should contain a list of 20 flyers within 10 kilometers of -37.769, 144.979


