﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using LiberisLabs.CompaniesHouse.Request;
using LiberisLabs.CompaniesHouse.Response.CompanySearch;
using LiberisLabs.CompaniesHouse.UriBuilders;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace LiberisLabs.CompaniesHouse.Tests
{
    [TestFixture]
    public class CompanyHouseSearchCompanyClientTests
    {
        private CompanyHouseSearchCompanyClient _client;

        private CompaniesHouseClientResponse<CompanySearch> _result;
        private ResourceDetails _resourceDetails;
        private List<CompanyDetails> _expectedCompanies;


        [TestFixtureSetUp]
        public void GivenACompanyHouseSearchCompanyClient_WhenSearchingForACompany()
        {
            var fixture = new Fixture();
            _resourceDetails = fixture.Create<ResourceDetails>();
            _expectedCompanies = new List<CompanyDetails>
            {
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "active").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "dissolved").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "liquidation").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "receivership").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "administration").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "voluntary-arrangement").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "converted-closed").With(x => x.CompanyType, "private-unlimited").Create(),
                fixture.Build<CompanyDetails>().With(x => x.CompanyStatus, "insolvency-proceedings").With(x => x.CompanyType, "private-unlimited").Create(),
            };

            var uri = new Uri("https://wibble.com/search/companies");

            var resource = new CompanySearchResourceBuilder()
                .AddCompanies(_expectedCompanies)
                .CreateResource(_resourceDetails);

            HttpMessageHandler handler = new StubHttpMessageHandler(uri, resource);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateHttpClient())
                .Returns(new HttpClient(handler));

            var uriBuilder = new Mock<ICompanySearchUriBuilder>();
            uriBuilder.Setup(x => x.Build(It.IsAny<CompanySearchRequest>()))
                .Returns(uri);

            _client = new CompanyHouseSearchCompanyClient(httpClientFactory.Object, uriBuilder.Object);

            _result = _client.SearchCompany(new CompanySearchRequest()).Result;
        }

        [Test]
        public void ThenTheRootIsCorrect()
        {
            Assert.That(_result.Data.ETag, Is.EqualTo(_resourceDetails.ETag));
            Assert.That(_result.Data.ItemsPerPage, Is.EqualTo(_resourceDetails.ItemsPerPage));
            Assert.That(_result.Data.Kind, Is.EqualTo(_resourceDetails.Kind));
            Assert.That(_result.Data.PageNumber, Is.EqualTo(_resourceDetails.PageNumber));
            Assert.That(_result.Data.StartIndex, Is.EqualTo(_resourceDetails.StartIndex));
            Assert.That(_result.Data.TotalResults, Is.EqualTo(_resourceDetails.TotalResults));
        }

        [Test]
        public void ThenTheCompaniesAreCorrect()
        {
            Assert.That(_result.Data.Companies.Count(), Is.EqualTo(8));

            foreach (var actual in _result.Data.Companies)
            {
                var companyDetails = _expectedCompanies.First(x => x.CompanyNumber == actual.CompanyNumber);

                Assert.That(actual.CompanyNumber, Is.EqualTo(companyDetails.CompanyNumber));

                Assert.That(actual.Address.AddressLine1, Is.EqualTo(companyDetails.AddressLine1));
                Assert.That(actual.Address.AddressLine2, Is.EqualTo(companyDetails.AddressLine2));
                Assert.That(actual.Address.CareOf, Is.EqualTo(companyDetails.CareOf));
                Assert.That(actual.Address.Country, Is.EqualTo(companyDetails.Country));
                Assert.That(actual.Address.Locality, Is.EqualTo(companyDetails.Locality));
                Assert.That(actual.Address.PoBox, Is.EqualTo(companyDetails.PoBox));
                Assert.That(actual.Address.PostalCode, Is.EqualTo(companyDetails.PostalCode));
                Assert.That(actual.Address.Region, Is.EqualTo(companyDetails.Region));

                Assert.That(actual.CompanyStatus, Is.EqualTo(ExpectedCompanyStatus[companyDetails.CompanyStatus]));
                Assert.That(actual.CompanyType, Is.EqualTo(ExpectedCompanyType[companyDetails.CompanyType]));
                Assert.That(actual.DateOfCessation, Is.EqualTo(companyDetails.DateOfCessation.Date));
                Assert.That(actual.DateOfCreation, Is.EqualTo(companyDetails.DateOfCreation.Date));
                Assert.That(actual.Description, Is.EqualTo(companyDetails.Description));
                Assert.That(actual.Kind, Is.EqualTo(companyDetails.Kind));
                Assert.That(actual.Links.Self, Is.EqualTo(companyDetails.LinksSelf));
                Assert.That(actual.Matches.Title, Is.EqualTo(companyDetails.MatchesTitle));
                Assert.That(actual.Snippet, Is.EqualTo(companyDetails.Snippet));
                Assert.That(actual.Title, Is.EqualTo(companyDetails.Title));
            }
        }

        private static readonly IReadOnlyDictionary<string, CompanyStatus> ExpectedCompanyStatus = new Dictionary
            <string, CompanyStatus>()
        {
            {"active", CompanyStatus.Active},
            {"dissolved", CompanyStatus.Dissolved},
            {"liquidation", CompanyStatus.Liquidation},
            {"receivership", CompanyStatus.Receivership},
            {"administration", CompanyStatus.Administration},
            {"voluntary-arrangement", CompanyStatus.VoluntaryArrangement},
            {"converted-closed", CompanyStatus.ConvertedClosed},
            {"insolvency-proceedings", CompanyStatus.InsolvencyProceedings}
        };

        private static readonly IReadOnlyDictionary<string, CompanyType> ExpectedCompanyType = new Dictionary
    <string, CompanyType>()
        {
            {"private-unlimited", CompanyType.PrivateUnlimited}
        };
    }
}
