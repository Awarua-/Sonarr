﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest
    {
        [Test]
        public void RefreshEpisodeInfo()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes).Verifiable();

            //mocker.GetMock<IRepository>().SetReturnsDefault();


            //Act
            var sw = Stopwatch.StartNew();
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(seriesId);
            var actualCount = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId);
            //Assert
            mocker.GetMock<TvDbProvider>().VerifyAll();
            Assert.Count(episodeCount, actualCount);
            Console.WriteLine("Duration: " + sw.Elapsed);
        }

        [Test]
        public void IsNeededTrue()
        {
            //Setup
            var season = new Mock<SeasonProvider>();
            var series = new Mock<SeriesProvider>();
            //var history = new Mock<IHistoryProvider>();
            //var quality = new Mock<IQualityProvider>();
            var repo = new Mock<IRepository>();

            var epInDb = new Episode
                             {
                                 AirDate = DateTime.Today,
                                 EpisodeId = 55555,
                                 EpisodeNumber = 5,
                                 Language = "en",
                                 SeasonId = 4444,
                                 SeasonNumber = 1
                             };

            season.Setup(s => s.IsIgnored(12345, 1)).Returns(false);
            series.Setup(s => s.QualityWanted(12345, QualityTypes.TV)).Returns(true);
            repo.Setup(s => s.Single<Episode>(c => c.SeriesId == 12345 && c.SeasonNumber == 1 && c.EpisodeNumber == 5)).
                Returns(epInDb);

            //repo.Setup(s => s.All<EpisodeFile>()).Returns();
            //repo.All<EpisodeFile>().Where(c => c.EpisodeId == episode.EpisodeId);


            //Act


            //Assert
        }


        [Test]
        public void Add_daily_show_episodes()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.Resolve<TvDbProvider>();
            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("c:\\test\\", tvDbSeriesId, 0);
            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(tvDbSeriesId);

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            Assert.IsNotEmpty(episodes);
        }
    }
}